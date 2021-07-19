using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
//using System.Runtime.InteropServices;
using NeuronPerformance.Models;
using NetSim.Data.Files;
using CodePerformance;

namespace NeuronPerformance
{
    class Program
    {
        /*[DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool QueryProcessCycleTime(IntPtr ProcessHandle, out ulong CycleTime);*/

        public static void PrintHelp()
        {
            Console.WriteLine("usage:");
            Console.WriteLine("{0} [nSamples=N] [totalTime=T] [maxTime=M] [tolerance=R] [-w] [-h]", System.IO.Path.GetFileName(System.Environment.GetCommandLineArgs()[0]));
            Console.WriteLine("-");
            Console.WriteLine("-w = write files containing the membrane potential of the neurons");
            Console.WriteLine("-h,-help = print help");
            Console.WriteLine("nSamples = amount of times that the simulation of each model will run from 1 to T seconds");
            Console.WriteLine("maxTime = maximum time (in timesteps of the model) to test for convergence to the fixed point");
            Console.WriteLine("tolerance = tolerance used to define if the model has converged to the fixed point");
            Console.WriteLine("totalTime = time to run the model (in ms, considering 1 spike = 1 ms), it will be converted to timesteps of the model according to the rules:");
            Console.WriteLine("    KTzTanh           -> 100 ts = 10 ms");
            Console.WriteLine("    KTzLog            -> 100 ts = 10 ms");
            Console.WriteLine("    Izhikevich        -> 100 ts = 20 ms");
            Console.WriteLine("    Rulkov            -> 100 ts = 10 ms");
            Console.WriteLine("    HHLeech (dt=0.01) -> 100 ts = 1 ms");
        }

        static void Main(string[] args)
        {
            NeuronRegime nr;
            ModelSimulator nm;
            Int32 nSamples = 10000;
            Int32 totalTime = 1000;
            Boolean writePotFile = false;
            Double tolerance = 1.0e-8;
            Int32 maxTime = 100000;

#if !DEBUG
            try
            {
#endif
            if (args.Length > 0)
            {
                foreach (String arg in args)
                {
                    String[] parVal = arg.Split(new char[] { '=' });
                    if (parVal[0][0] == '-') parVal[0] = parVal[0].Substring(1);
                    if (parVal[0] == "nSamples")
                        nSamples = Convert.ToInt32(parVal[1]);
                    else if (parVal[0] == "totalTime")
                        totalTime = Convert.ToInt32(parVal[1]);
                    else if (parVal[0] == "w")
                        writePotFile = true;
                    else if (parVal[0] == "tolerance")
                        tolerance = Convert.ToDouble(parVal[1]);
                    else if (parVal[1] == "maxTime")
                        maxTime = Convert.ToInt32(parVal[1]);
                    else if ((parVal[0] == "h") || (parVal[0] == "help"))
                    {
                        Program.PrintHelp();
                        return;
                    }
                    else
                        throw new ArgumentOutOfRangeException(String.Format("Unrecognized parameter! {0}", parVal[0]));
                }
            }
#if !DEBUG
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR!");
                Console.WriteLine(e.Message);
                Program.PrintHelp();
                return;
            }
#endif

            nm = new ModelSimulator(NeuronRegime.Bursting);
            //nm.RunModel(nm.HHLeechODE, totalTime, 0.0D);
            CodeTimer.Time(true, nm.HHLeechODE.ToString(), totalTime, nm.HHLeechODE.TimeStep);
            //nm = new ModelSimulator(NeuronRegime.Excitable);
            //nm.RunModel(nm.HHLeechODE, totalTime, 0.0D);
            /*
            Console.WriteLine("Preparing to measure timestep time...");
            nr = NeuronRegime.Bursting;
            nm = new ModelSimulator(nr);
            System.Threading.Thread.Sleep(3000);

            Console.WriteLine("Beginning...");
            Console.WriteLine("-");

            if (writePotFile)
            {
                nm.RunModel(nm.KTzLogMap, totalTime);//, 0.0D);
                nm.RunModel(nm.KTzTanhMap, totalTime);//, 0.0D);
                nm.RunModel(nm.RulkovMap, totalTime);//, 0.0D);
                nm.RunModel(nm.IzhikevichMap, totalTime);//, 0.0D);
                nm.RunModel(nm.HHLeechODE, totalTime);//, 0.0D);
            }

            nm.RunTimeStepTest(nm.KTzTanhMap, nSamples, totalTime);//, 0.0D);
            nm.RunTimeStepTest(nm.KTzLogMap, nSamples, totalTime);//, 0.0D);
            nm.RunTimeStepTest(nm.IzhikevichMap, nSamples, totalTime);//, 0.0D);
            nm.RunTimeStepTest(nm.RulkovMap, nSamples, totalTime);//, 0.0D);
            nm.RunTimeStepTest(nm.HHLeechODE, nSamples, totalTime);//, 0.0D);

            Console.WriteLine("--");
            Console.WriteLine("Preparing to measure convergence time...");
            nr = NeuronRegime.Excitable;
            nm = new ModelSimulator(nr);
            System.Threading.Thread.Sleep(3000);

            Console.WriteLine("Beginning...");
            Console.WriteLine("-");

            if (writePotFile)
            {
                nm.RunModel(nm.KTzLogMap, totalTime);//, 0.0D);
                nm.RunModel(nm.KTzTanhMap, totalTime);//, 0.0D);
                nm.RunModel(nm.RulkovMap, totalTime);//, 0.0D);
                nm.RunModel(nm.IzhikevichMap, totalTime);//, 0.0D);
                nm.RunModel(nm.HHLeechODE, totalTime);//, 0.0D);
                nm.ResetModels();
            }

            nm.RunFPConvergenceTest(nm.KTzLogMap, nSamples, maxTime, tolerance);//, 0.0D);
            nm.RunFPConvergenceTest(nm.KTzTanhMap, nSamples, maxTime, tolerance);//, 0.0D);
            nm.RunFPConvergenceTest(nm.IzhikevichMap, nSamples, maxTime, tolerance);//, 0.0D);
            nm.RunFPConvergenceTest(nm.RulkovMap, nSamples, maxTime, tolerance);//, 0.0D);
            nm.RunFPConvergenceTest(nm.HHStdODE, nSamples, maxTime, tolerance);//, 0.0D);

            
            */
#if DEBUG
            Console.ReadKey();
#endif
        }
    }

    public class Statistics
    {
        public Double StdDev { get; private set; }
        public Double RndErr { get; private set; }
        public Double Average { get; private set; }
        public Double[] Data { get; private set; }
        
        public Statistics(Double[] data)
        {
            this.Data = data;
            Int32 tot = data.Length;
            Int32 i = 0;
            Double avg = data.Average();
            Double stdDev = 0.0D;
            Double rndErr = 0.0D;
            while (i < tot)
            {
                stdDev += (data[i] - avg) * (data[i] - avg);
                i++;
            }
            stdDev /= (Double)tot - 1.0D;
            stdDev = Math.Sqrt(stdDev);
            rndErr = stdDev / Math.Sqrt(tot);
            this.Average = avg;
            this.StdDev = stdDev;
            this.RndErr = rndErr;
        }
    }

    public class ModelSimulator
    {
        public NeuronRegime neuronRegime { get; private set; }
        public KTzLogModel KTzLogMap { get; private set; }
        public KTzTanhModel KTzTanhMap { get; private set; }
        public RulkovModel RulkovMap { get; private set; }
        public IzhikevichModel IzhikevichMap { get; private set; }
        public HHLeechModel HHLeechODE { get; private set; }
        public HHStdModel HHStdODE { get; private set; }

        public ModelSimulator(NeuronRegime nr)
        {
            this.neuronRegime = nr;
            this.KTzLogMap = new KTzLogModel(nr);
            this.KTzTanhMap = new KTzTanhModel(nr);
            this.RulkovMap = new RulkovModel(nr);
            this.IzhikevichMap = new IzhikevichModel(nr);
            this.HHLeechODE = new HHLeechModel(nr);
            this.HHStdODE = new HHStdModel(nr);
            this.ResetModels();
        }

        private void RunTransient(IModel model)
        {
            Int32 t = 0, tTotal = (Int32)((Double)model.transient / model.dt) + 1;
            while (t < tTotal)
            {
                //model.TimeStep(0.0D);
                model.TimeStep();
                t++;
            }
        }

        private Statistics TestTimeStepModel(IModel model, Int32 nSamples, Int32 totalTime)//, Double Iext)
        {
            Int32 i, t;
            Int32 mTotalTime = (Int32)((Double)model.ts_per_ms * (Double)totalTime);
            Double[] executionTicks = new Double[nSamples];
            Stopwatch sw = new Stopwatch();
            for (i = 0; i < nSamples; i++)
            {
                t = 0;
                sw.Start();
                while (t < mTotalTime)
                {
                    //model.TimeStep(Iext);
                    model.TimeStep();
                    t++;
                }
                sw.Stop();
                executionTicks[i] = (Double)sw.ElapsedTicks / (Double)mTotalTime;
                sw.Reset();
            }
            return new Statistics(executionTicks);
        }

        private Statistics[] TestFPConvergenceModel(IModel model, Int32 nSamples, Int32 maxTime, Double tolerance)//, Double Iext)
        {
            Statistics[] res = new Statistics[2];
            Int32 i, t;
            Int32 mTime = (Int32)Math.Ceiling((Double)maxTime / model.dt);
            Double[] realTime = new Double[nSamples];
            Double[] modelTime = new Double[nSamples];
            Stopwatch sw = new Stopwatch();
            for (i = 0; i < nSamples; i++)
            {
                t = 0;
                Boolean found = false;
                model.Reset(this.neuronRegime);
                Double xAnt = model.GetV();
                sw.Start();
                while (t < mTime)
                {
                    //model.TimeStep(Iext);
                    model.TimeStep();
                    if (Math.Abs(model.GetV() - xAnt) < tolerance)
                    {
                        found = true;
                        break;
                    }
                    xAnt = model.GetV();
                    t++;
                }
                sw.Stop();
                if (found)
                {
                    realTime[i] = (Double)sw.ElapsedTicks / (Double)t;
                    modelTime[i] = (Double)t;
                }
                else
                {
                    throw new ArgumentException(String.Format("The model could not converge to the fixed point. Model: {0}; MaxTimeSteps: {1}; Tolerance: {2:0.00000000e+000}.", model.ToString(), mTime, tolerance));
                }
                sw.Reset();
            }
            res[0] = new Statistics(realTime);
            res[1] = new Statistics(modelTime);
            return res;
        }

        public void RunFPConvergenceTest(IModel model, Int32 nSamples, Int32 maxTime, Double tolerance)//, Double Iext)
        {
            System.Threading.Thread.Sleep(1000);
            Console.WriteLine("* {0} Model:", model.ToString());
            Statistics[] executionStat = this.TestFPConvergenceModel(model, nSamples, maxTime, tolerance);//, Iext);
            Console.WriteLine("  - Real world convergence time (ns) = {0} +/- {1}", executionStat[0].Average * 100.0D, executionStat[0].StdDev * 100.0D);
            Console.WriteLine("  - Model convergence time (ts)      = {0} +/- {1}", executionStat[1].Average, executionStat[1].StdDev);
            Console.WriteLine("-");
        }

        public void RunTimeStepTest(IModel model, Int32 nSamples, Int32 totalTime)//, Double Iext)
        {
            System.Threading.Thread.Sleep(1000);
            Console.WriteLine("* {0} Model:", model.ToString());
            Statistics executionStat = this.TestTimeStepModel(model, nSamples, totalTime);//, Iext);
            Console.WriteLine("  - Execution time (ns) = {0} +/- {1}", executionStat.Average * 100.0D, executionStat.StdDev * 100.0D); // 1 tick = 100 ns; so converting to ms: 1 ms = 10000 ticks
            Console.WriteLine("-");
        }

        public void RunModel(IModel model, Int32 totalTime)//, Double Iext)
        {
            Int32 mTotalTime = (Int32)((Double)model.ts_per_ms * (Double)totalTime);
            Double[] tData = new Double[mTotalTime];
            Double[] xData = new Double[mTotalTime];

            Int32 t = 0;
            while (t < mTotalTime)
            {
                //model.TimeStep(Iext);
                model.TimeStep();
                tData[t] = (Double)t * model.dt;
                xData[t] = model.GetV();
                t++;
            }

            String suffix = (this.neuronRegime == NeuronRegime.Bursting? "bst" : "exc" );
            OutputFile of = new OutputFile(model.ToString() + "_" + suffix + ".dat");
            of.WriteData("0.00000000e+000", "\t", "#t\tV", true, tData, xData);
            of.Close();
        }

        public void ResetModels()
        {
            this.KTzLogMap.Reset(this.neuronRegime);
            this.KTzTanhMap.Reset(this.neuronRegime);
            this.RulkovMap.Reset(this.neuronRegime);
            this.IzhikevichMap.Reset(this.neuronRegime);
            this.HHLeechODE.Reset(this.neuronRegime);
            this.HHStdODE.Reset(this.neuronRegime);

            if (this.neuronRegime == NeuronRegime.Bursting)
            {
                this.RunTransient(this.KTzTanhMap);
                this.RunTransient(this.KTzLogMap);
                this.RunTransient(this.RulkovMap);
                this.RunTransient(this.IzhikevichMap);
                this.RunTransient(this.HHLeechODE);
                this.RunTransient(this.HHStdODE);
            }
        }
    }
}
