using RestAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Xml.Linq;
using System.Collections;
using System.Net.Http;
using static System.Net.Mime.MediaTypeNames;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace RestAPI.Controllers
{
    public class ModulesController : ApiController
    {
        string connString = RestAPI.Properties.Settings.Default.ConnStr;
        
        // PUT: api/application/5
        [Route("api/somiod/modules/{id:int}/update")]
        public IHttpActionResult Put(int id, [FromBody] Module value)
        {
            SqlConnection conn = null;          //classe que permite aceder a base de dados
            try
            {
                conn = new SqlConnection(connString);     //initialize the connection class
                conn.Open();
                string sql = "UPDATE Resources SET Name = @name WHERE id = @id and res_type = 'module'";
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


        [Route("api/somiod/modules/{application}/{module}/delete")]
        public IHttpActionResult Delete(string application, string module)
        {
                                                                                               
           string sql = "DELETE FROM Resources WHERE (parent = (SELECT id FROM Resources WHERE name=@module)) OR " + //delete children
                "(name = @module AND res_type = 'module' AND parent = (SELECT id FROM Resources WHERE name=@application))"; //delete the module, verify it's parent is the correct app


            SqlConnection conn = new SqlConnection(connString);
           SqlCommand cmd = new SqlCommand(sql, conn);

            try
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@application", application);
                cmd.Parameters.AddWithValue("@module", module);
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
                    System.Diagnostics.Debug.WriteLine("ERROR 1 in delete: " + exception.ToString());
                    return InternalServerError(exception);
                }
            }

            System.Diagnostics.Debug.WriteLine("ERROR 2 in delete!");
            return InternalServerError();
        }

        
        [Route("api/somiod/modules")]
        public HttpResponseMessage Get()
        {
            List<Module> modules = new List<Module>();
            
            int moduleId = 0;
            string sql = "SELECT * FROM Resources WHERE res_type = 'module' ORDER BY Id";            
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connString);
                conn.Open();

                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                      Module module = new Module
                    {                        
                        Id = (int)reader["Id"],
                        Name = (string)reader["Name"],
                        Creation_dt = (string)reader["Creation_dt"],
                        Parent = (int)reader["Parent"]
                    };                    
                    modules.Add(module);
                }
                reader.Close();
                conn.Close();
                 foreach (Module module in modules)
                {
                    conn = new SqlConnection(connString);
                    conn.Open();
                    module.Data = new List<Data>();
                    moduleId = module.Id;
                    string sqlData = "SELECT * FROM Resources WHERE res_type = 'data' AND parent = "+ moduleId +" ORDER BY Id";
                    SqlCommand cmdData = new SqlCommand(sqlData, conn);
                    SqlDataReader readerData = cmdData.ExecuteReader();
                    while (readerData.Read())
                    {

                        Data dataRow = new Data
                        {
                            Id = (int)readerData["Id"],
                            Content = (string)readerData["Content"],
                            Creation_dt = (string)readerData["Creation_dt"],
                            Parent = (int)readerData["Parent"]
                        };

                        module.Data.Add(dataRow);
                        //add row to list of data
                    }
                    conn.Close();
                }
                
            }
            catch (Exception)
            {
                //fechar ligação à base de dados
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            var xmlSerializer = new XmlSerializer(typeof(List<Module>));
            var stringWriter = new StringWriter();
            xmlSerializer.Serialize(stringWriter, modules);
            var xml = stringWriter.ToString();
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(xml, Encoding.UTF8, "application/xml");
            return response;
            //return modules;
        }



        [Route("api/somiod/modules/{id}")]
        public HttpResponseMessage Get(int id)
        {
            SqlConnection conn = null;
            try
            {
                string sqlModule = "SELECT * FROM Resources WHERE res_type = 'module' AND id = @id ORDER BY Id";
                Module module = null;
                Data dataRow = null;
                conn = new SqlConnection(connString);
                conn.Open();
                SqlCommand cmd = new SqlCommand(sqlModule, conn);
                cmd.Parameters.AddWithValue("@id", id);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {

                    module = new Module
                    {
                        Id = (int)reader["Id"],
                        Name = (string)reader["Name"],
                        Creation_dt = (string)reader["Creation_dt"],
                        Parent = (int)reader["Parent"]

                    };
                    module.Data = new List<Data>();

                }
                reader.Close();
                string sqlData = "SELECT * FROM Resources WHERE res_type = 'data' AND parent = @id ORDER BY Id";
                SqlCommand cmdData = new SqlCommand(sqlData, conn);
                cmdData.Parameters.AddWithValue("@id", id);
                SqlDataReader readerData = cmdData.ExecuteReader();
                while (readerData.Read())
                {

                    dataRow = new Data
                    {
                        Id = (int)readerData["Id"],
                        Content = (string)readerData["Content"],
                        Creation_dt = (string)readerData["Creation_dt"],
                        Parent = (int)readerData["Parent"]
                    };
                    //add row to list of data
                    module.Data.Add(dataRow);
                }

                conn.Close();

                if (module == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Module not found");
                }
                var xmlSerializer = new XmlSerializer(typeof(Module));
                var stringWriter = new StringWriter();
                xmlSerializer.Serialize(stringWriter, module);
                var xml = stringWriter.ToString();
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(xml, Encoding.UTF8, "application/xml");
                return response;
                //                return Ok(module);

            }
            catch (Exception)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            return null;
        }


        [Route("api/somiod/{application}/modules")]
        public HttpResponseMessage Get(string application)
        {
            List<Module> modules = new List<Module>();            
            int moduleId = 0;
            string sql = "SELECT m.* FROM Resources m WHERE m.parent = (SELECT id FROM Resources WHERE name = @application AND res_type='application')";

            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connString);
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@application", application);
                SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                {

                    Module module = new Module
                    {
                        Id = (int)reader["Id"],
                        Name = (string)reader["Name"],
                        Creation_dt = (string)reader["Creation_dt"],
                        Parent = (int)reader["Parent"]
                    };
                    modules.Add(module);
                }


                if (modules.Count == 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Modules not found");
                }
                reader.Close();
                conn.Close();
                foreach (Module module in modules)
                {
                    conn = new SqlConnection(connString);
                    conn.Open();
                    module.Data = new List<Data>();
                    moduleId = module.Id;
                    string sqlData = "SELECT * FROM Resources WHERE res_type = 'data' AND parent = " + moduleId + " ORDER BY Id";
                    SqlCommand cmdData = new SqlCommand(sqlData, conn);
                    SqlDataReader readerData = cmdData.ExecuteReader();
                    while (readerData.Read())
                    {

                        Data dataRow = new Data
                        {
                            Id = (int)readerData["Id"],
                            Content = (string)readerData["Content"],
                            Creation_dt = (string)readerData["Creation_dt"],
                            Parent = (int)readerData["Parent"]
                        };

                        module.Data.Add(dataRow);
                        //add row to list of data
                    }
                    conn.Close();
                }
            }
            catch (Exception)
            {
                //fechar ligação à base de dados
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }

            }
            var xmlSerializer = new XmlSerializer(typeof(List<Module>));
            var stringWriter = new StringWriter();
            xmlSerializer.Serialize(stringWriter, modules);
            var xml = stringWriter.ToString();
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(xml, Encoding.UTF8, "application/xml");
            return response;
            //return modules;
        }

        
        //POST create module
        [Route("api/somiod/{application}/create")]
        public HttpResponseMessage Post(string application, [FromBody] Module value) {

            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connString);
                conn.Open();

                //GET application ID (parent)
                string sqlParent = "SELECT id FROM Resources WHERE name = @name AND res_type = @restype "; 
                SqlCommand cmdParent = new SqlCommand(sqlParent, conn);
                cmdParent.Parameters.AddWithValue("@name", application);
                cmdParent.Parameters.AddWithValue("@restype", "application");

                int parentID = 0;

                SqlDataReader reader = cmdParent.ExecuteReader();

                if(reader.Read()) 
                {
                    parentID = (int)reader["id"];
                }
                reader.Close();

                if (parentID == 0) { 
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Parent application not found");
                }

                //CREATE
                string sql = "INSERT INTO Resources(res_type, name, creation_dt, parent) VALUES (@res_type, @name, @creation_dt, @parent)";
                SqlCommand cmd = new SqlCommand(sql, conn);

                if (!value.Res_type.Equals("module"))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Res_type must be module to post a module");
                }

                cmd.Parameters.AddWithValue("@res_type", value.Res_type);
                cmd.Parameters.AddWithValue("@name", value.Name);
                value.Creation_dt = DateTime.UtcNow.ToString("yyyy-MM-dd HH\\:mm\\:ss", CultureInfo.InvariantCulture);
                cmd.Parameters.AddWithValue("@creation_dt", value.Creation_dt);
                cmd.Parameters.AddWithValue("@parent", parentID);

                int numRows = cmd.ExecuteNonQuery();
                conn.Close();
                if (numRows == 1)
                {
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error creating module");
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