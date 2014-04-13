using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Zeptomoby.OrbitTools;

namespace Tirit.Server
{

    public class SatelitesController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<SateliteInfo> Get()
        {
            string timeSecsParam = Request.GetQueryNameValuePairs().Where(c => c.Key == "t").Select(c => c.Value).FirstOrDefault();

            int timeSecs;


            int.TryParse(timeSecsParam, out timeSecs);

            var inputFile = HttpContext.Current.Server.MapPath("~/sat.txt");
            using (var reader = new StreamReader(inputFile))
            {
                var name = reader.ReadLine();
                while (!string.IsNullOrEmpty(name))
                {
                    var line1 = reader.ReadLine();

                    if (string.IsNullOrEmpty(line1))
                    {
                        break;
                    }

                    var line2 = reader.ReadLine();

                    if (string.IsNullOrEmpty(line2))
                    {
                        break;
                    }

                    Tle tle = new Tle(name, line1, line2);

                    Orbit orbit = new Orbit(tle);
                    List<Eci> coords = new List<Eci>();

                    var step = 1;

                    // Calculate position, velocity
                    // mpe = "minutes past epoch"
                    //for (int mpe = 0; mpe <= (step * 4); mpe += step)
                    //{
                    //    // Get the position of the satellite at time "mpe".
                    //    // The coordinates are placed into the variable "eci".
                    //    Eci eci = orbit.GetPosition(mpe);

                    //    // Add the coordinate object to the list
                    //    coords.Add(eci);
                    //}

                    double mpe;

                    double.TryParse(tle.Epoch, out mpe);

                    mpe += timeSecs / 60.0;

                    Eci eci = orbit.GetPosition(mpe);

                    yield return new SateliteInfo
                    {
                        Name = name,
                        Position = eci.Position,
                        Speed = eci.Velocity
                    };

                    //yield return null;

                    name = reader.ReadLine();

                }
            }
        }

        // GET api/<controller>/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST api/<controller>
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT api/<controller>/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE api/<controller>/5
        //public void Delete(int id)
        //{
        //}
    }
}