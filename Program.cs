using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;

namespace DigitalOceanIndicator
{
    class Program
    {

        NotifyIcon systemTrayIcon;
        ContextMenu contextMenu;
        MenuItem exitItem, dropletsItem, menuStrip;
        string getDropletsRequest;
        string rebootDropletRequest;
        string clientID;
        string apiKey;
        string responseString;
        List<Droplet> droplets;

        static void Main(string[] args)
        {
            Program program = new Program();
            Application.Run();
            Console.ReadLine();
        }

        Program()
        {
            systemTrayIcon = new NotifyIcon();
            contextMenu = new ContextMenu();
            exitItem = new MenuItem();
            dropletsItem = new MenuItem();
            menuStrip = new MenuItem();

            clientID = "";                      /*Put your client id here */
            apiKey = "";                        /*Put your api key here */
            getDropletsRequest = string.Format("https://api.digitalocean.com/droplets/?client_id={0}&api_key={1}", clientID, apiKey);

            Console.WriteLine(getDropletsRequest);

            systemTrayIcon.Text = "Digital Ocean Indicator";

            systemTrayIcon.Icon = new Icon("favicon(1).ico");

            systemTrayIcon.ContextMenu = contextMenu;

            systemTrayIcon.Visible = true;

            systemTrayIcon.Click += new EventHandler(this.systemTrayIconClick);

            exitItem.Text = "Exit";
            exitItem.Click += new EventHandler(exitItemClick);

            dropletsItem.Text = "Droplets";
            dropletsItem.Click += new EventHandler(refreshItemClick);


            contextMenu.MenuItems.Add(exitItem);
            contextMenu.MenuItems.Add(dropletsItem);

            requestDropletStatus();
        }

        public void systemTrayIconClick(object sender, EventArgs e)
        {

        }

        public void exitItemClick(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public void refreshItemClick(object sender, EventArgs e)
        {
            refreshContextMenu();
            requestDropletStatus();
        }

        public void refreshContextMenu()
        {
            contextMenu.MenuItems.Clear();
            contextMenu.MenuItems.Add(exitItem);
            contextMenu.MenuItems.Add(dropletsItem);
        }

        public void requestDropletStatus()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(getDropletsRequest);

            request.Timeout = 5000;
            request.UserAgent = "Digital Ocean Indicator";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            StreamReader responseStream = new StreamReader(response.GetResponseStream());

            responseString = responseStream.ReadToEnd();

            ResponseHelper responseHelper = breakUpThisFuckingJson(responseString);
            droplets = responseHelper.droplets;

            foreach (Droplet d in droplets)
            {
                MenuItem mi = new MenuItem();
                mi.Text = d.name;
                mi.Click += new EventHandler(dropletItemClick);
                mi.Tag = d;
                contextMenu.MenuItems.Add(mi);

                MenuItem ipAddressButton = new MenuItem();
                MenuItem statusButton = new MenuItem();
                MenuItem rebootButton = new MenuItem();

                statusButton.Text = d.status;
                ipAddressButton.Text = d.ip_address;
                rebootButton.Text = "Reboot";
                rebootButton.Tag = d;
                rebootButton.Click += new EventHandler(rebootButtonClick);

                mi.MenuItems.Add(statusButton);
                mi.MenuItems.Add(ipAddressButton);
                mi.MenuItems.Add(rebootButton);
            }

            response.Close();
            responseStream.Close();
        }

        public ResponseHelper breakUpThisFuckingJson(string stringToFuckUp)
        {
            ResponseHelper o = JsonConvert.DeserializeObject<ResponseHelper>(stringToFuckUp);

            return o;
        }

        public void dropletItemClick(object sender, EventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            Droplet d = (Droplet) mi.Tag;

            Console.WriteLine(d.id);
        }

        public void rebootButtonClick(object sender, EventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            Droplet d = (Droplet) mi.Tag;

            rebootDropletRequest = String.Format("https://api.digitalocean.com/droplets/{0}/reboot/?client_id={1}&api_key={2}", d.id, clientID, apiKey);
            Console.WriteLine(rebootDropletRequest);


            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(rebootDropletRequest);

            request.Timeout = 5000;
            request.UserAgent = "Digital Ocean Indicator";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            StreamReader responseStream = new StreamReader(response.GetResponseStream());

            responseString = responseStream.ReadToEnd();

            Console.WriteLine(responseString);

            response.Close();
            responseStream.Close();
        }
    }
}
