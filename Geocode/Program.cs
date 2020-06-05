using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Configuration;
using Geocode.Code;
using Geocode.Data;

namespace Geocode
{
    class Program
    {
        static UniversalEntities entity = new UniversalEntities();
        
        
        static void Main(string[] args)
        {
            //List<Supplier> suppliers = entity.Suppliers.Where(p => p.IsNetworkSupplier.Value).ToList();
            List<SupplierAddress> supplierAddresses = entity.SupplierAddresses.Where(p => !p.IsPostalAddress.Value).ToList();
            Console.WriteLine("Found " + supplierAddresses.Count() + " Network Suppliers");
            try
            {
                int i = 0;
                int d = 0;
                foreach (var item in supplierAddresses)
                {
                    i++;
                    Console.WriteLine(i + "Getting GeoLocation for " + item.IdSupplier);

                    Console.WriteLine(i + "Getting GeoLoc for " + item.Address2 + "," + item.Address3 + "," + item.Address4);
                    Province prov = entity.Provinces.Where(p => p.IdProvince == item.IdProvince).FirstOrDefault();
                    GeoLoc? loc = Geocoder.LocateGoogle(item.Address2 + "," + item.Address3 + "," + item.Address4 + "," + prov.Name);
                    if (loc.HasValue)
                    {
                        Console.WriteLine(i + "Supplier " + item.IdSupplier + loc.Value.ToString());
                        item.Latitude = loc.Value.Lat;
                        item.Longitude = loc.Value.Lon;
                        d++;
                    }
                    else
                    {
                        Console.WriteLine(i + "No co-ordinates found for supplier " + item.IdSupplier);
                    }
                }
                    /*
                    //SupplierAddress address = entity.SupplierAddresses.Where(p => !p.IsPostalAddress.Value && p.IdSupplier == item.IdSupplier).FirstOrDefault();                    
                   // if (address != null)
                    //{
                        //Province prov = entity.Provinces.Where(p => p.IdProvince == address.IdProvince).FirstOrDefault();
                        //if (prov != null)
                        //{
                        //    if (prov.IdProvince == 11)
                        /*    {
                                prov = null;
                            }                            
                        }
                        if (!address.Latitude.HasValue)
                        {
                         * 
                            Console.WriteLine(i + "Getting GeoLoc for " + address.Address1 + "," + address.Address2 + "," + (prov != null ? prov.Name: "") + "," + (address.PostalCode != null ? address.PostalCode.Trim(): ""));
                            GeoLoc? loc = Geocoder.LocateGoogle(address.Address1 + "," + address.Address2 + "," + (prov != null ? prov.Name : "") + "," + (address.PostalCode != null ? address.PostalCode.Trim() : ""));
                            if (loc.HasValue)
                            {
                                Console.WriteLine(i + "Supplier " + item.SupplierName + loc.Value.ToString());
                                item.Latitude = loc.Value.Lat;
                                item.Longitude = loc.Value.Lon;
                                d++;
                            }
                            else
                            {
                                Console.WriteLine(i + "No co-ordinates found for supplier " + item.SupplierName);
                            }
                        }
                        else
                        {
                            Console.WriteLine(i + "Supplier " + item.SupplierName + " has co-ordinates and is skipped");
                        }
                    }
                    else
                    {
                        Console.WriteLine(i + "Could not find an address for " + item.SupplierName);
                    }
                }
                     * */
                entity.SaveChanges();
                Console.WriteLine(d + "has been updated");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            // List<CareCrossDoctors> doctors = entity.CareCrossDoctors.ToList();      
            
        }

        private static void LocateGoogle()
        {
            throw new NotImplementedException();
        }

        //step 1: get geocodes
        private static string GeoCodeDatabase(string city, string state)
        {
            string url = "http://maps.google.com/maps/geo?output=xml&key=" + ConfigurationManager.AppSettings["GoogleMapsKey"] + "&q=" + HttpUtility.UrlEncode(city + " " + state);
            WebRequest req = HttpWebRequest.Create(url);
            WebResponse res = req.GetResponse();
            StreamReader sr = new StreamReader(res.GetResponseStream());
            try
            {
                Match coord = Regex.Match(sr.ReadToEnd(), "<coordinates>.*</coordinates>");
                if (!coord.Success) return "";
                return coord.Value.Substring(13, coord.Length - 27);
            }
            finally
            {
                sr.Close();
            }
        }

        //step 2: 

        private void CheckWithin5kmRadius(string address)
        {
            // Find the START LOCATION (entered by the user)
            GeoLoc startLocation = Geocoder.LocateGoogle(address+ "," + ConfigurationManager.AppSettings["Country"]) ?? new GeoLoc();
            SortedList<Double, Supplier> nearest = new SortedList<double, Supplier>();

            List<Supplier> doctors = entity.Suppliers.Where(i => i.IsNetworkSupplier.Value && i.IdDiscipline == 10 || i.IdDiscipline == 11).ToList();

            foreach (Supplier o in doctors)
            {
                GeoLoc location = new GeoLoc
                {
                    Lat = o.Latitude.Value,
                    Lon = o.Longitude.Value
                };
                if (location.Lat != 0 && location.Lon != 0)
                {
                    Double distance = Distance(startLocation, location);
                    if (distance > 0 && distance <= 15)
                    {
                        Console.WriteLine("Doctor found within 15 km");
                    }
                }
            }

            if (startLocation.Lon != 0)
            {   // starting point was resolved
            }
        }

        private double Distance(GeoLoc location1, GeoLoc location2)
        {
            return CDistanceBetweenLocations.Calc(location1.Lat, location1.Lon, location2.Lat, location2.Lon);
        }
    }
}
