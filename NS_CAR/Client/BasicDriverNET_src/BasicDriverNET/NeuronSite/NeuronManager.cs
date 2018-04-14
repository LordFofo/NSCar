using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuronSite
{
    public class NeuronManager
    {
        private Random r = new Random();
        public List<double> Weight { get; set; }
        public List<double[]> NeuronLayers { get; set; }
        public List<double[]> Input { get; set; }
        public List<double[]> Output { get; set; }
        private Fce function { get; set; }

        public NeuronManager(string Input, Fce fce)
        {
            function = fce;
            NeuronLayers = new List<double[]>();
            int[] values = Input.Split('-').Select(x => Convert.ToInt32(x)).ToArray();

            for (int i = 0; i < values.Count(); i++)
            {
                double[] Layer = new double[values[i]];
                NeuronLayers.Add(Layer);
            }
            #region tested
            //Weight = new List<double>();
            //Weight.Add(0.15);
            //Weight.Add(0.20);
            //Weight.Add(0.35);

            //Weight.Add(0.25);
            //Weight.Add(0.30);
            //Weight.Add(0.35);

            //Weight.Add(0.4);
            //Weight.Add(0.45);
            //Weight.Add(0.6);

            //Weight.Add(0.50);
            //Weight.Add(0.55);
            //Weight.Add(0.6);
            #endregion

        }

        private void GenerateWeight()
        {
            int Last = NeuronLayers.Count();
            Weight = new List<double>();

            for (int i = 1; i < Last; i++)
            {
                int numberInLayer = NeuronLayers[i].Count();

                for (int k = 0; k < numberInLayer; k++)
                {

                    for (int j = 0; j < NeuronLayers[i - 1].Count(); j++)
                    {
                        NeuronLayers[i - 1][j] = 1;
                        double weight = r.Next(-1000000, 1000000) / 1000000.0;
                        Weight.Add(weight);
                    }
                    //bias
                    double w = r.Next(-1000000, 1000000) / 1000000.0;
                    Weight.Add(w);
                }
            }


        }

        public void SetTraningData(List<double[]> Inputs, List<double[]> Outputs)
        {
            Output = new List<double[]>(Outputs);

            Input = new List<double[]>(Inputs);

            //foreach (var item in Inputs)
            //{
            //    double[] current = new double[item.Length + 1];
            //    for (int i = 0; i < item.Length; i++)
            //        current[i] = item[i];
            //    current[item.Length] = 1;
            //    Input.Add(current);
            //}
        }

        public void LearnByRandom(double maxError)
        {
            int weightCount = Weight.Count();

            double GlobalError = double.MaxValue;
            while (GlobalError > maxError)
            {
                GlobalError = 0;
                GenerateWeight();
                for (int x = 0; x < Input.Count; x++)
                {
                    NeuronLayers[0] = Input[x];
                    GetError();
                    GlobalError += AbsError(Output[x]);
                }

                Console.WriteLine(GlobalError);
            }

            Console.WriteLine(GlobalError);
            foreach (var item in Weight)
            {
                Console.WriteLine(item);
            }

        }

        public void GetError()
        {
            int weightIndex = 0;
            int Last = NeuronLayers.Count();

            for (int i = 1; i < Last; i++)
            {
                int numberInLayer = NeuronLayers[i].Count();

                for (int k = 0; k < numberInLayer; k++)
                {
                    double currentValue = 0;
                    for (int j = 0; j < NeuronLayers[i - 1].Count(); j++)
                    {
                        currentValue += ((double)Weight[weightIndex++] * (double)NeuronLayers[i - 1][j]);
                    }
                    //bias
                    currentValue += Weight[weightIndex++];
                    NeuronLayers[i][k] = currentValue.Binary(function);
                }
            }

        }

        public double AbsError(double[] v)
        {
            double result = 0;

            for (int i = 0; i < v.Count(); i++)
            {
                result += Math.Abs(NeuronLayers.Last()[i] - v[i]);
            }

            return result;
        }

        private double[] PowError(double[] v)
        {
            double[] result = new double[v.Length];

            for (int i = 0; i < v.Count(); i++)
            {
                result[i] = Math.Pow(NeuronLayers.Last()[i] - v[i], 2) / 2;
            }
            return result;
        }

        public double LearnByBackPropagation(int Epoch, double Lambda)
        {
            GenerateWeight();
            double GlobalError = 0;
            do
            {
                for (int x = 0; x < Input.Count; x++)
                {
                    NeuronLayers[0] = Input[x];

                    GetError();
                    double[] Etotal = PowError(Output[x]);
                    int weighIndex = Weight.Count() - 1;
                    int Last = NeuronLayers.Count() - 1;

                    double[] increment = new double[weighIndex + 1];

                    #region First Layer
                    for (int i = NeuronLayers[Last].Count() - 1; i > -1; i--)
                    {
                        double curentWeight = 1;
                        double err = NeuronLayers.Last()[i] - Output[x][i];
                        double log = NeuronLayers.Last()[i] * (1 - NeuronLayers.Last()[i]);
                        double total = err * log * curentWeight;
                        increment[weighIndex--] = total;

                        for (int j = NeuronLayers[Last - 1].Count() - 1; j > -1; j--)
                        {
                            double curentWeight1 = NeuronLayers[NeuronLayers.Count() - 2][i];
                            double err1 = NeuronLayers.Last()[i] - Output[x][i];
                            double log1 = NeuronLayers.Last()[i] * (1 - NeuronLayers.Last()[i]);
                            double total1 = err1 * log1 * curentWeight1;
                            increment[weighIndex--] = total1;
                        }
                    }
                    #endregion

                    #region ErrorOutput

                    int backIndex = weighIndex - 1;
                    List<double> sumsError = new List<double>();
                    for (int w = 0; w < NeuronLayers[NeuronLayers.Count() - 2].Count(); w++)
                    {
                        List<double> er = new List<double>();
                        for (int i = 0; i < Output[x].Count(); i++)
                        {
                            double err = NeuronLayers.Last()[i] - Output[x][i];
                            double log = NeuronLayers.Last()[i] * (1 - NeuronLayers.Last()[i]);
                            double ww = (Weight[backIndex]);
                            double y = err * log * ww;
                            er.Add(y);
                            backIndex += Output[x].Count() + 1;
                        }
                        sumsError.Add(er.Sum());
                        backIndex = weighIndex;
                    }

                    #endregion

                    #region Other Layers
                    for (int yy = NeuronLayers.Count() - 2; yy > 0; yy--)
                    {

                        for (int i = NeuronLayers[yy].Count() - 1; i > -1; i--)
                        {
                            double curentWeight = 1;
                            double err = sumsError[i];
                            double log = NeuronLayers[yy][i] * (1 - NeuronLayers[yy][i]);
                            double total = err * log * curentWeight;
                            increment[weighIndex--] = total;

                            for (int j = NeuronLayers[yy - 1].Count() - 1; j > -1; j--)
                            {
                                double curentWeight1 = NeuronLayers[yy - 1][j];
                                double err1 = sumsError[i];
                                double log1 = NeuronLayers[yy][i] * (1 - NeuronLayers[yy][i]);
                                double total1 = err1 * log1 * curentWeight1;
                                increment[weighIndex--] = total1;

                            }
                        }

                        #region Error Hidden Layers
                        backIndex = weighIndex;
                        List<double> NewSumError = new List<double>();
                        for (int w = 0; w < NeuronLayers[yy - 1].Count(); w++)
                        {
                            List<double> er = new List<double>();
                            for (int k = 0; k < NeuronLayers[yy].Count() - 1; k++)
                            {
                                double y = (sumsError[k]) * (Weight[++backIndex]);
                                er.Add(y);
                            }
                            NewSumError.Add(er.Sum());
                        }
                        sumsError.Clear(); sumsError.AddRange(NewSumError);
                        #endregion

                    }
                    #endregion


                    for (int i = 0; i < Weight.Count(); i++)
                    {
                        Weight[i] -= increment[i] * Lambda;
                    }
                }
                //GlobalError = 0;
                //for (int x = 0; x < Input.Count; x++)
                //{
                //    NeuronLayers[0] = Input[x];
                //    GetError();
                //    GlobalError += AbsError(Output[x]);
                //}
                //Console.WriteLine("Epoch :{0}  Global_Error:{1}", Epoch, GlobalError);

            } while (Epoch-- > 0);

            for (int x = 0; x < Input.Count; x++)
            {
                NeuronLayers[0] = Input[x];
                GetError();
                GlobalError += AbsError(Output[x]);
            }
            Console.WriteLine(" Global_Error:{0}", GlobalError);
            return GlobalError;
        }

        public void TestData(List<double[]> I)
        {
            function = Fce.Binary;

            for (int x = 0; x < I.Count; x++)
            {
                NeuronLayers[0] = I[x];
                GetError();
                string result = "InPut:\t";
                foreach (var item in I[x])
                {
                    result += item + " ";
                }

                string result2 = "Calculate:";
                foreach (var item in NeuronLayers.Last())
                {
                    result2 += item + " ";
                }
                Console.WriteLine("{0}   {1}", result, result2);
            }

            function = Fce.Logistic;

        }



        public double[] GetValues(double[] In)
        {

            function = Fce.Logistic;
            List<double> result = new List<double>();

            NeuronLayers[0] = In;
            GetError();

            foreach (var item in NeuronLayers.Last())
            {
                result.Add(item);
            }

            return result.ToArray();
        }
    }



}
