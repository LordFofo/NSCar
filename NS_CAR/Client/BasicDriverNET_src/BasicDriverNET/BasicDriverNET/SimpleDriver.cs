using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicDriverNET
{
    class SimpleDriver : IDriver
    {
		public Dictionary<String, float> drive(Dictionary<String, float> values) {
			Dictionary<String, float> responses = new Dictionary<String, float>();

            //float distance0 = values["distance0"];
            //// pokud je v levo jede doprava, jinak do leva
            //if (distance0 < 0.5) {
            //	responses.Add("wheel", 0.8f);
            //} else {
            //	responses.Add("wheel", 0.2f);
            //}
            //// maximalni zrychleni
            //responses.Add("acc", 1.5f);

            string Magic = string.Empty;
            List<double> Weight = new List<double>();
            using (StreamReader r = new StreamReader("NeuronWeight.txt"))
            {
               Magic = r.ReadLine();
               string row = r.ReadLine();

               while(row!=null)               
               {
                    Weight.Add(double.Parse(row));
                    row = r.ReadLine();
                }              
            }

            NeuronManager nm = new NeuronManager(Magic,Fce.Logistic);

            nm.Weight = Weight;
            List<double> Input=values.Select(x => (double)x.Value).ToList();


            var OutPut=nm.GetValues(Input.ToArray());

            responses.Add("wheel", (float)OutPut.First());
            responses.Add("acc", (float)OutPut.Last());

            return responses;
		}




    }
}
