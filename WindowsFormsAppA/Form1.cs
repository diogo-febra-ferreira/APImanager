using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Xml.Serialization;

namespace WindowsFormsAppA
{
    public partial class Form1 : Form
    {

        MqttClient mClient;
        MqttClient mClientLamp;

        string baseURI = @"http://localhost:44313"; 

        public Form1()
        {
            InitializeComponent();
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBoxTypes.Items.Clear();
            comboBoxTypes.Items.Add("Applications");
            comboBoxTypes.Items.Add("Modules");
            comboBoxGetByID.Items.Clear();
            comboBoxGetByID.Items.Add("Applications");
            comboBoxGetByID.Items.Add("Modules");
        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private async void buttonUpdateApp_Click(object sender, EventArgs e)
        {
            string txtBoxID = textBoxIDApp.Text;

            if (txtBoxID.Length == 0)
            {
                MessageBox.Show("You must enter a valid ID! Cannot be empty.", "Error handling the request", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                if (!(Int32.TryParse(txtBoxID, out int id)))
                {
                    MessageBox.Show("You must enter a valid ID! Must be a positive integer. [" + txtBoxID + "]", "Error handling the request", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else if (Int32.Parse(txtBoxID) < 1)
                {
                    MessageBox.Show("You must enter a valid ID! Must be a positive integer. [" + txtBoxID + "]", "Error handling the request", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else
                {
                    HttpClient client = new HttpClient();
                    var idR = int.Parse(textBoxIDApp.Text);
                    client.BaseAddress = new Uri("https://localhost:44313/api/somiod/applications/" + idR + "/update");
                    var content = new StringContent(JsonConvert.SerializeObject(new Application
                    {
                        Id = int.Parse(textBoxIDApp.Text),
                        Name = textBoxNameAppUpdate.Text
                    }), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PutAsync("", content);
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Success");
                    }
                    else
                    {
                        MessageBox.Show(response.StatusCode.ToString());
                    }
                }
            }
        }

        private async void buttonUpdateModule_Click(object sender, EventArgs e)
        {
            
            string txtBoxID = textBoxIDModuleUpdate.Text;

            if (txtBoxID.Length == 0)
            {
                MessageBox.Show("You must enter a valid ID! Cannot be empty.", "Error handling the request", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                if (!(Int32.TryParse(txtBoxID, out int id)))
                {
                    MessageBox.Show("You must enter a valid ID! Must be a positive integer. [" + txtBoxID + "]", "Error handling the request", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else if (Int32.Parse(txtBoxID) < 1)
                {
                    MessageBox.Show("You must enter a valid ID! Must be a positive integer. [" + txtBoxID + "]", "Error handling the request", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else
                {
                    HttpClient client = new HttpClient();
                    var idR = int.Parse(textBoxIDModuleUpdate.Text);
                    client.BaseAddress = new Uri("https://localhost:44313/api/somiod/modules/" + idR + "/update");
                    var content = new StringContent(JsonConvert.SerializeObject(new Module
                    {
                        Id = int.Parse(textBoxIDModuleUpdate.Text),
                        Name = textBoxNameModuleUpdate.Text
                    }), Encoding.UTF8, "application/json");



                    HttpResponseMessage response = await client.PutAsync("", content);
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Success");
                    }
                    else {
                        MessageBox.Show(response.StatusCode.ToString());
                    }
                }
            }
        }

        private async void buttonShowAllTypes_Click(object sender, EventArgs e) //GET ALL APPS/MODULES
        {
            
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:44313/api/somiod/");
             if (comboBoxTypes.SelectedIndex == 0)
             {
                 client.BaseAddress = new Uri("https://localhost:44313/api/somiod/applications");

             }else if(comboBoxTypes.SelectedIndex == 1)
             {
                 client.BaseAddress = new Uri("https://localhost:44313/api/somiod/modules");
            }
            else
            {
                MessageBox.Show("You must select a resource!","Error handling the request", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;

            }

            HttpResponseMessage response = await client.GetAsync("");

            string data = await response.Content.ReadAsStringAsync();
            
            Form form = new Form();
            form.Width = 600;
            form.Height = 400;

            System.Windows.Forms.TextBox textBox = new System.Windows.Forms.TextBox();
            textBox.Multiline = true;
            textBox.ScrollBars = ScrollBars.Vertical;
            textBox.Dock = DockStyle.Fill;
            textBox.Text = data;

            // Add the TextBox to the form and show the form
            form.Controls.Add(textBox);
            form.Show();
        }

        private async void btnGetByID_Click_1(object sender, EventArgs e)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:44313/api/somiod/");
            if (comboBoxGetByID.SelectedIndex == 0)
            {
                client.BaseAddress = new Uri("https://localhost:44313/api/somiod/applications");

            }
            else if (comboBoxGetByID.SelectedIndex == 1)
            {
                client.BaseAddress = new Uri("https://localhost:44313/api/somiod/modules");
            }
            else
            {
                MessageBox.Show("You must select a resource!", "Error handling the request", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;

            }

            string txtBoxID = textBoxGetByID.Text;

            if (txtBoxID.Length == 0)
            {
                MessageBox.Show("You must enter a valid ID! Cannot be empty.", "Error handling the request", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                if (!(Int32.TryParse(txtBoxID, out int id)))
                {
                    MessageBox.Show("You must enter a valid ID! Must be a positive integer. [" + txtBoxID + "]", "Error handling the request", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else if (Int32.Parse(txtBoxID) < 1)
                {
                    MessageBox.Show("You must enter a valid ID! Must be a positive integer. [" + txtBoxID + "]", "Error handling the request", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else
                {
                    HttpResponseMessage response = await client.GetAsync(client.BaseAddress + "/" + txtBoxID);
                    string data = await response.Content.ReadAsStringAsync();
                    Form form = new Form();
                    form.Width = 600;
                    form.Height = 400;

                    System.Windows.Forms.TextBox textBox = new System.Windows.Forms.TextBox();
                    textBox.Multiline = true;
                    textBox.ScrollBars = ScrollBars.Vertical;
                    textBox.Dock = DockStyle.Fill;
                    textBox.Text = data;

                    // Add the TextBox to the form and show the form
                    form.Controls.Add(textBox);
                    form.Show();
                }
            }
        }

        private async void btnGetByAppName_Click_1(object sender, EventArgs e)
        {

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:44313/api/somiod");

            string txtBoxAppName = textBoxGetByAppName.Text;

            if (txtBoxAppName.Length == 0)
            {
                MessageBox.Show("You must enter a valid name! Cannot be empty.", "Error handling the request", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
            else
            {
                HttpResponseMessage response = await client.GetAsync(client.BaseAddress + "/" + txtBoxAppName + "/modules");
                string data = await response.Content.ReadAsStringAsync();
                Form form = new Form();
                form.Width = 600;
                form.Height = 400;

                System.Windows.Forms.TextBox textBox = new System.Windows.Forms.TextBox();
                textBox.Multiline = true;
                textBox.ScrollBars = ScrollBars.Vertical;
                textBox.Dock = DockStyle.Fill;
                textBox.Text = data;

                // Add the TextBox to the form and show the form
                form.Controls.Add(textBox);
                form.Show();
            }

        }

        private async void buttonDelete_Click(object sender, EventArgs e)
        {
            string appToDelete = textBoxDeleteApplication.Text;
            string modToDelete = textBoxDeleteModule.Text;
            string subToDelete = textBoxDeleteSubscription.Text;
            string dataToDelete = textBoxDeleteDataId.Text;
            string address = "", type = "ERROR";

            System.Diagnostics.Debug.WriteLine("DELETE 1 - a/m/s/d : " + appToDelete + "/" + modToDelete + "/" + subToDelete + "/" + dataToDelete);

            HttpClient client = new HttpClient();

            if (subToDelete != "")
            {
                //[Route("api/somiod/{application}/{module}/subscriptions/{subscription}")]
                address = "https://localhost:44313/api/somiod/" + appToDelete + "/" + modToDelete + "/subscriptions/" + subToDelete + "/delete";
                type = "subscription " + subToDelete;
            }
            else if (dataToDelete != "")
            {
                //[Route("api/somiod/{application}/{module}/data/{data}")]
                address = "https://localhost:44313/api/somiod/" + appToDelete + "/" + modToDelete + "/data/" + dataToDelete + "/delete";
                type = "data " + dataToDelete;
            }
            else if (modToDelete != "")
            {
                //[Route("api/somiod/modules/{application}/{module}")]
                address = "https://localhost:44313/api/somiod/modules/" + appToDelete + "/" + modToDelete + "/delete";
                type = "module " + modToDelete;
            }
            else if (appToDelete != "")
            {

                //[Route("api/somiod/applications/{application}")]
                address = "https://localhost:44313/api/somiod/applications/" + appToDelete + "/delete";
                type = "application " + appToDelete;
            }

            System.Diagnostics.Debug.WriteLine("DELETE 2 url - " + address);




            client.BaseAddress = new Uri(address);
            HttpResponseMessage response = await client.DeleteAsync(address);
            //System.Diagnostics.Debug.WriteLine("DELETE 3 response - " + response.ToString());
            System.Diagnostics.Debug.WriteLine("DELETE 3 - " + response.StatusCode.ToString());

            if (response.StatusCode.ToString() == "OK")
            {
                System.Windows.Forms.MessageBox.Show("'" + type + "' was deleted succesfully");
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("ERROR: there was a problem deleting '" + type + "'...");
            }

        }

        private async void buttonSubscribe_Click(object sender, EventArgs e)
        {
            //post subscription
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:44313/api/somiod/lighting/light_bulb/create");
            var content = new StringContent(JsonConvert.SerializeObject(new Subscription
            {
                Res_type = "subscription",
                Name = "sub1",
                Event = "creation",
                Endpoint = "mqtt://127.0.0.1",
            }), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("", content);
            if (!response.IsSuccessStatusCode)
            {
                string message = await response.Content.ReadAsStringAsync();
                MessageBox.Show("Error creating subscription resource. Error: " + response.
                    StatusCode + " " + message);
                return;
            }
            //subscribe channel in mqtt

            mClientLamp = new MqttClient(IPAddress.Parse("127.0.0.1"));
            mClientLamp.MqttMsgPublishReceived += client_MqttMsgPublishReceivedLamp;
            mClientLamp.Connect(Guid.NewGuid().ToString());
            if (!mClientLamp.IsConnected)
            {
                MessageBox.Show("Error connecting to message broker...");
                return;
            }
            byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE}; //QoS – depends on the topics number
            string[] topics = { "light_bulb" }; 
            mClientLamp.Subscribe( topics , qosLevels);

        }

        private void client_MqttMsgPublishReceivedLamp(object sender, MqttMsgPublishEventArgs e)
        {
            if (buttonLamp.BackColor == Color.Khaki)
            {
                buttonLamp.BackColor = Color.Yellow;
            }
            else {
                buttonLamp.BackColor = Color.Khaki;
            }
        }

        private void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            MessageBox.Show("Message received from broker: " + Encoding.UTF8.GetString(e.Message));
        }

        private async void buttonON_Click(object sender, EventArgs e)
        {
            //post data resource
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:44313/api/somiod/lighting/light_bulb/create");


            XmlSerializer serializer = new XmlSerializer(typeof(string));
            StringWriter stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, "flip" );
            string xmlString = stringWriter.ToString();


            var content = new StringContent(JsonConvert.SerializeObject(new Data
            {
                Res_type = "data",
                Content = xmlString,

            }), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("", content);
            if (!response.IsSuccessStatusCode)
            {
                string message = await response.Content.ReadAsStringAsync();
                MessageBox.Show("Error creating data resource. Error: " + response.StatusCode + " " + message);
            }
        }

        private async void buttonCreateModule_Click(object sender, EventArgs e)
        {
            if (textBoxAppModule.Text.Equals("")) {
                MessageBox.Show("Type the name of the application in which you want to create a module");
                return;
            }

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:44313/api/somiod/" + textBoxAppModule.Text + "/create");
            var content = new StringContent(JsonConvert.SerializeObject(new Module
            {
                Res_type = "module",
                Name = textBoxNameModule.Text
            }), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("", content);
            if (!response.IsSuccessStatusCode)
            {
                string message = await response.Content.ReadAsStringAsync();
                MessageBox.Show("Error creating module resource. Error: " + response.
                    StatusCode + " " + message);
            }
            else
            {
                MessageBox.Show("Success.");
            }
        }

        private async void buttonCreateSubscription_Click(object sender, EventArgs e)
        {
            if (textBoxApplicationSubscriptionData.Text.Equals("") || textBoxModuleSubscriptionData.Text.Equals(""))
            {
                MessageBox.Show("Type the name of the application and module in which you want to create a subscription");
                return;
            }

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:44313/api/somiod/" + textBoxApplicationSubscriptionData.Text + "/"+ textBoxModuleSubscriptionData.Text + "/create");
            var content = new StringContent(JsonConvert.SerializeObject(new Subscription
            {
                Res_type = "subscription",
                Name = textBoxNameSubscription.Text,
                Event = textBoxEventSubscription.Text,
                Endpoint= textBoxEndpointSubscription.Text,
            }), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("", content);
            if (!response.IsSuccessStatusCode)
            {
                string message = await response.Content.ReadAsStringAsync();
                MessageBox.Show("Error creating subscription resource. Error: " + response.
                    StatusCode + " " + message);
                return;
            }
            //subscribe channel in mqtt
            try
            {
                var endpoint = textBoxEndpointSubscription.Text.Substring(7).Split(':');
                mClient = new MqttClient(IPAddress.Parse(endpoint[0]));
                mClient.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
                mClient.Connect(Guid.NewGuid().ToString());
                if (!mClient.IsConnected)
                {
                    MessageBox.Show("Error connecting to message broker...");
                    return;
                }
                byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE }; //QoS – depends on the topics number
                string[] topics = { textBoxModuleSubscriptionData.Text };
                mClient.Subscribe(topics, qosLevels);
            }
            catch (Exception exc) { 
                MessageBox.Show(exc.Message);
                return;
            }
            MessageBox.Show("Success");
        }

        private async void buttonCreateData_Click(object sender, EventArgs e)
        {
            if (textBoxApplicationSubscriptionData.Text.Equals("") || textBoxModuleSubscriptionData.Text.Equals(""))
            {
                MessageBox.Show("Type the name of the application and module in which you want to create a data resource");
                return;
            }

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:44313/api/somiod/" + textBoxApplicationSubscriptionData.Text + "/" + textBoxModuleSubscriptionData.Text+ "/create");
            XmlSerializer serializer = new XmlSerializer(typeof(string));
            StringWriter stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, textBoxContentData.Text);
            string xmlString = stringWriter.ToString();


            var content = new StringContent(JsonConvert.SerializeObject(new Data
            {
                Res_type = "data",
                Content = xmlString,

            }), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("", content);
            if (!response.IsSuccessStatusCode)
            {
                string message = await response.Content.ReadAsStringAsync();
                MessageBox.Show("Error creating data resource. Error: " + response.
                    StatusCode + " " + message);
            }
            else
            {
                MessageBox.Show("Success.");
            }
        }

        private async void buttonCreateApp_Click_1(object sender, EventArgs e)
        {
            if (textBoxNameApp.Text.Equals(""))
            {
                MessageBox.Show("Type the name of the application you want to create");
                return;
            }


            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:44313/api/somiod/create");
            var content = new StringContent(JsonConvert.SerializeObject(new Application
            {
                Res_type = "application",
                Name = textBoxNameApp.Text
            }), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("", content);
            if (!response.IsSuccessStatusCode)
            {
                string message = await response.Content.ReadAsStringAsync();
                MessageBox.Show("Error creating application resource. Error: " + response.
                    StatusCode + " " + message);
            }
            else
            {
                MessageBox.Show("Success.");
            }
        }
    }
}
