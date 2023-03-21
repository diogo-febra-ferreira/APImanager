
using RestAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Services.Description;
using static System.Net.Mime.MediaTypeNames;
using Application = RestAPI.Models.Application;
using System.Xml.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.IO;
using System.Xml;

namespace RestAPI.Controllers
{
    public class ApplicationsController : ApiController
    {
        //{domain:port}/api/somiod/...

        string connString = RestAPI.Properties.Settings.Default.ConnStr;
        // PUT: api/application/5
        [HttpPut, Route("api/somiod/applications/{id:int}/update")]
        public IHttpActionResult Put(int id, [FromBody] Application value)
        {
            SqlConnection conn = null;          //classe que permite aceder a base de dados
            try
            {
                conn = new SqlConnection(connString);     //initialize the connection class
                conn.Open();
                string sql = "UPDATE Resources SET Name = @name WHERE id = @id and res_type = 'application'";
                SqlCommand cmd = new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@name", value.Name);

                int numRows = cmd.ExecuteNonQuery();
                conn.Close();
                if (numRows == 1)
                {
                    return Ok();
                }
                return InternalServerError();
            }
            catch (Exception e)
            {

                //Fechar a ligacao a BD
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
                return Content(HttpStatusCode.InternalServerError, e.Message);
            }

        }


        [Route("api/somiod/applications/{application}/delete")]
        public IHttpActionResult Delete(string application)
        {
                                                         
            string sql = "DELETE FROM Resources WHERE " +
                "(parent IN (SELECT id FROM Resources WHERE parent=(SELECT id FROM Resources WHERE name=@application))) OR " +//delete data and subs
                "(parent = (SELECT id FROM Resources WHERE name=@application)) OR " +//delete modules
                "(name = @application AND res_type = 'application')"; //delete app


            SqlConnection conn = new SqlConnection(connString);
            SqlCommand cmd = new SqlCommand(sql, conn);

            try
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@application", application);
                int deletedRows = cmd.ExecuteNonQuery();
                cmd.Dispose();
                conn.Close();

                if (deletedRows > 0)
                {
                    System.Diagnostics.Debug.WriteLine("DELETE successful: deleted " + deletedRows + " rows.");
                    return Ok();

                }
                return NotFound();


            }
            catch (Exception exception)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    cmd.Dispose();
                    conn.Close();
                    System.Diagnostics.Debug.WriteLine("ERROR in delete: " + exception.ToString());
                    return InternalServerError(exception);
                }
            }

            return InternalServerError();
        }

        [Route("api/somiod/applications")]
        public HttpResponseMessage Get()
        {
            List<Application> applications = new List<Application>();
            string sql = "SELECT * FROM Resources WHERE res_type = 'application' ORDER BY Id";
            //return null;
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connString);
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Application application = new Models.Application
                    {                        
                        Id = (int)reader["Id"],
                        Name = (string)reader["Name"],
                        Creation_dt = (string)reader["Creation_dt"]
                    };
                    applications.Add(application);
                }
            
                reader.Close();
                conn.Close();
            }
            catch (Exception)
            {
                //fechar ligação à base de dados
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            var xmlSerializer = new XmlSerializer(typeof(List<Application>));
            var stringWriter = new StringWriter();
            //var xmlWriterSettings = new XmlWriterSettings { Indent = true };
            //var xmlWriter = XmlWriter.Create(stringWriter, xmlWriterSettings);
            xmlSerializer.Serialize(stringWriter, applications);
            var xml = stringWriter.ToString();
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(xml, Encoding.UTF8, "application/xml");
            return response;
            
        }

        
        [Route("api/somiod/applications/{id}")]
        public HttpResponseMessage Get(int id)
        {
           
            string sql = "SELECT * FROM Resources WHERE res_type = 'application' AND id = @id ORDER BY Id";
            Application application = null;

            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connString);
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);     

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    application = new Application
                    {                        
                        Id = (int)reader["Id"],
                        Name = (string)reader["Name"],
                        Creation_dt = (string)reader["Creation_dt"]
                    };
                }
                reader.Close();
                conn.Close();
                
                if (application == null)
                {
                    //return NotFound();
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Application not found");

                }
                var xmlSerializer = new XmlSerializer(typeof(Application));
                var stringWriter = new StringWriter();
                xmlSerializer.Serialize(stringWriter, application);
                var xml = stringWriter.ToString();
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(xml, Encoding.UTF8, "application/xml");
                return response;
                //return Ok(application);   

            }
            catch (Exception)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Application not found");
            
            //return NotFound();
        }

        [Route("api/somiod/create")]
        public HttpResponseMessage Post([FromBody] Application value)
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connString);
                conn.Open();
                string sql = "INSERT INTO Resources(res_type, name, creation_dt) VALUES (@res_type, @name, @creation_dt)";
                SqlCommand cmd = new SqlCommand(sql, conn);

                if (!value.Res_type.Equals("application"))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Res_type must be application to post an application");
                }

                cmd.Parameters.AddWithValue("@res_type", value.Res_type);
                cmd.Parameters.AddWithValue("@name", value.Name);
                value.Creation_dt = DateTime.UtcNow.ToString("yyyy-MM-dd HH\\:mm\\:ss", CultureInfo.InvariantCulture);
                cmd.Parameters.AddWithValue("@creation_dt", value.Creation_dt);

                int numRows = cmd.ExecuteNonQuery();
                conn.Close();
                if (numRows == 1)
                {
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error creating application");
            }
            catch (Exception e)
            {
                //Fechar a ligacao a BD
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                    
                }
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }
          
    }
}
    