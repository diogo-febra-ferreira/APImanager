using RestAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Web.Http;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Exceptions;

namespace RestAPI.Controllers
{
    public class SubscriptionsController : ApiController
    {
        string connString = RestAPI.Properties.Settings.Default.ConnStr;
        MqttClient mClient;
        //POST create subscription/data

        [HttpPost, Route("api/somiod/{application}/{module}/create")]
        public HttpResponseMessage Post(string application, string module, [FromBody] SubscriptionAndData value)
        {

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

                if (reader.Read())
                {
                    parentID = (int)reader["id"];
                }
                reader.Close();

                if (parentID == 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Parent application not found");
                }

                //GET module ID (parent)
                string sqlParentModule = "SELECT id FROM Resources WHERE name = @name AND res_type = @restype AND parent = @parentID";
                SqlCommand cmdParentModule = new SqlCommand(sqlParentModule, conn);
                cmdParentModule.Parameters.AddWithValue("@name", module);
                cmdParentModule.Parameters.AddWithValue("@restype", "module");
                cmdParentModule.Parameters.AddWithValue("@parentID", parentID);

                int parentIDModule = 0;

                SqlDataReader readerModule = cmdParentModule.ExecuteReader();

                if (readerModule.Read())
                {
                    parentIDModule = (int)readerModule["id"];
                }
                readerModule.Close();

                if (parentIDModule == 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Parent module not found");
                }



                //verify res_type
                if (value.Res_type.Equals("subscription"))
                {

                    if (!(value.Event.Equals("creation") || value.Event.Equals("deletion")))
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid event, event allowed : creation or deletion");
                    }

                    //CREATE
                    string sql = "INSERT INTO Resources(res_type, name, creation_dt, parent, event, endpoint) VALUES (@res_type, @name, @creation_dt, @parent, @event, @endpoint)";
                    SqlCommand cmd = new SqlCommand(sql, conn);

                    //validate endpoint
                    if (!value.Endpoint.Substring(0, 7).Equals("mqtt://"))
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid endpoint, only mqtt endpoints allowed");
                    }


                    cmd.Parameters.AddWithValue("@res_type", value.Res_type);
                    cmd.Parameters.AddWithValue("@name", value.Name);
                    value.Creation_dt = DateTime.UtcNow.ToString("yyyy-MM-dd HH\\:mm\\:ss", CultureInfo.InvariantCulture);
                    cmd.Parameters.AddWithValue("@creation_dt", value.Creation_dt);
                    cmd.Parameters.AddWithValue("@parent", parentIDModule);
                    cmd.Parameters.AddWithValue("@event", value.Event);
                    var endpoint = value.Endpoint.Substring(7).Split(':');
                    cmd.Parameters.AddWithValue("@endpoint", endpoint[0]);


                    int numRows = cmd.ExecuteNonQuery();
                    conn.Close();
                    if (numRows == 1)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error creating subscription");
                }
                else if (value.Res_type.Equals("data"))
                {
                    //CREATE
                    string sql = "INSERT INTO Resources(res_type, creation_dt, parent, content) VALUES (@res_type, @creation_dt, @parent,@content)";
                    SqlCommand cmd = new SqlCommand(sql, conn);

                    //verify res_type
                    if (!value.Res_type.Equals("data"))
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "res_type must be type data");
                    }

                    if (value.Content.Equals(""))
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Content can not be empty");
                    }

                    cmd.Parameters.AddWithValue("@res_type", value.Res_type);
                    value.Creation_dt = DateTime.UtcNow.ToString("yyyy-MM-dd HH\\:mm\\:ss", CultureInfo.InvariantCulture);
                    cmd.Parameters.AddWithValue("@creation_dt", value.Creation_dt);
                    cmd.Parameters.AddWithValue("@parent", parentIDModule);

                    XmlSerializer serializer = new XmlSerializer(typeof(string));
                    StringReader stringReader = new StringReader(value.Content);
                    string content = serializer.Deserialize(stringReader).ToString();

                    cmd.Parameters.AddWithValue("@content", content);

                    int numRows = cmd.ExecuteNonQuery();
                    conn.Close();
                    if (numRows == 1)
                    {
                        //PUBLISH
                        sql = "SELECT * FROM Resources WHERE res_type = 'subscription' AND event='creation' AND parent=" + parentIDModule;
                        conn.Open();
                        cmd = new SqlCommand(sql, conn);

                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            try
                            {
                                string endpoint = (string)reader["endpoint"];

                                mClient = new MqttClient(IPAddress.Parse(endpoint));
                                mClient.Connect(Guid.NewGuid().ToString());
                                mClient.Publish(module, Encoding.UTF8.GetBytes("creation: " + value.Content));
                            }
                            catch (Exception e)
                            {
                                continue;
                            }
                        }
                        reader.Close();
                        conn.Close();


                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error creating data");
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Wrong Resource Type. Subscription OR Data");
                }
            }
            catch (Exception e)
            {
                //Fechar a ligacao a BD
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();

                }
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.ToString());
            }

        }

        [Route("api/somiod/{application}/{module}/subscriptions/{subscription}/delete")]
    public IHttpActionResult Delete(string application, string module, string subscription)
    {

        string sql = "DELETE FROM Resources WHERE name = @subscription AND res_type = 'subscription' " + //verify sub name
            "AND parent = (SELECT id FROM Resources WHERE name=@module) " + //verify module name
            "AND parent IN ( SELECT id FROM Resources WHERE parent=(SELECT id FROM resources WHERE name=@application))"; //verify app name
        SqlConnection conn = new SqlConnection(connString);
        SqlCommand cmd = new SqlCommand(sql, conn);
        try
        {
            conn.Open();
            cmd.Parameters.AddWithValue("@subscription", subscription);
            cmd.Parameters.AddWithValue("@module", module);
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
                    return InternalServerError();
                }
            }

            return InternalServerError();
        }   
    }
}