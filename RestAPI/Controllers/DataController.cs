using Newtonsoft.Json.Linq;
using RestAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Xml.Linq;
using System.Xml.Serialization;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Exceptions;

namespace RestAPI.Controllers
{
    public class DataController : ApiController
    {
        string connString = RestAPI.Properties.Settings.Default.ConnStr;
        MqttClient mClient;

        [HttpDelete ,Route("api/somiod/{application}/{module}/data/{data}/delete")]
        public HttpResponseMessage Delete(string application, string module, int data)
        {
            string deletedContent = null;
            string sqlDeleted = "SELECT * FROM Resources WHERE id = @data";
            SqlConnection connDeleted = new SqlConnection(connString);
            SqlCommand deletedCmd= new SqlCommand(sqlDeleted, connDeleted);
            int parentID = 0;

            try 
            {
                connDeleted.Open();
                deletedCmd.Parameters.AddWithValue("@data", data);
                SqlDataReader reader = deletedCmd.ExecuteReader();
                if (reader.Read())
                {
                    deletedContent = (string)reader["content"];
                    parentID = (int)reader["parent"];
                }
            
                deletedCmd.Dispose();
                connDeleted.Close();
            }
            catch (Exception e) 
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message);
            }


            string sql = "DELETE FROM Resources WHERE id = @data AND res_type = 'data' " + //verify sub name
                "AND parent = (SELECT id FROM Resources WHERE name=@module) " + //verify module name
                "AND parent IN ( SELECT id FROM Resources WHERE parent=(SELECT id FROM resources WHERE name=@application))"; //verify app name
            SqlConnection conn = new SqlConnection(connString);
            SqlCommand cmd = new SqlCommand(sql, conn);
            try
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@data", data);
                cmd.Parameters.AddWithValue("@module", module);
                cmd.Parameters.AddWithValue("@application", application);
                int deletedRows = cmd.ExecuteNonQuery();
                cmd.Dispose();
                conn.Close();

                if (deletedRows > 0)
                {
                    System.Diagnostics.Debug.WriteLine("DELETE successful: deleted " + deletedRows + " rows.");

                    //PUBLISH
                    string subSql = "SELECT * FROM Resources WHERE res_type = 'subscription' AND event='deletion' AND parent=" + parentID;
                    conn.Open();
                    cmd = new SqlCommand(subSql, conn);



                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        try
                        {
                            string endpoint = (string)reader["endpoint"];

                            mClient = new MqttClient(IPAddress.Parse(endpoint));
                            mClient.Connect(Guid.NewGuid().ToString());

                            XmlSerializer serializer = new XmlSerializer(typeof(string));
                            StringWriter stringWriter = new StringWriter();
                            serializer.Serialize(stringWriter, deletedContent);
                            string xmlString = stringWriter.ToString();

                            mClient.Publish(module, Encoding.UTF8.GetBytes("deletion: " + xmlString));
                        }
                        catch (MqttConnectionException e)
                        {
                            continue;
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK);

                }
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Row not found");


            }
            catch (Exception exception)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    cmd.Dispose();
                    conn.Close();
                    System.Diagnostics.Debug.WriteLine("ERROR in delete: " + exception.ToString());

                }
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error: " + exception.Message);
            }
        }



    }

}