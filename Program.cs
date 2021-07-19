using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using NeuronPerformance.Models;
using NetSim.Data.Files;
using CodePerformance;

namespace NeuronPerformance
{
    class Program
    {
        const Int32 DEFAULT_NSAMPLES = 100;
        const Int32 DEFAULT_TOTALTIME = 1000;
        const Boolean DEFAULT_WRITEPOTFILE = false;
        const Double DEFAULT_TOLERANCE = 1.0e-8;
        const Int32 DEFAULT_MAXTIME = 100000;
        const Int32 DEFAULT_N = 3;

        public static void PrintHelp()
        {
            Console.WriteLine("usage:");
            Console.WriteLine("{0} [nSamples=NUM] [totalTime=T] [maxTime=M] [tolerance=R] [N=NUM] [-w] [-h]", System.IO.Path.GetFileName(System.Environment.GetCommandLineArgs()[0]));
            Console.WriteLine("-");
            Console.WriteLine("-w        = (default = {0}) write files containing the membrane potential of the neurons", DEFAULT_WRITEPOTFILE);
            Console.WriteLine("-h,-help  = print help");
            Console.WriteLine("nSamples  = (default = {0}) amount of times that the simulation of each model will run from 1 to T seconds", DEFAULT_NSAMPLES);
            Console.WriteLine("maxTime   = (default = {0}) maximum time (in timesteps of the model) to test for convergence to the fixed point", DEFAULT_MAXTIME);
            Console.WriteLine("tolerance = (default = {0}) tolerance used to define if the model has converged to the fixed point", DEFAULT_TOLERANCE);
            Console.WriteLine("totalTime = (default = {0}) time to run the model (in ms, considering 1 spike = 1 ms)", DEFAULT_TOTALTIME);
            Console.WriteLine("N         = (default = {0}) number of neurons in the network", DEFAULT_N);
            Console.WriteLine("    totalTime will be converted to timesteps of the model according to the rules:");
            Console.WriteLine("    KTzTanh           -> 100 ts = 10 ms");
            Console.WriteLine("    KTzLog            -> 100 ts = 10 ms");
            Console.WriteLine("    GLExp             -> 1 ts   = 1 ms");
            Console.WriteLine("    Izhikevich        -> 100 ts = 20 ms");
            Console.WriteLine("    Rulkov            -> 100 ts = 10 ms");
            Console.WriteLine("    HHLeech (dt=0.01) -> 100 ts = 1 ms");
        }

        static void Main(string[] args)
        {
            NeuronRegime nr;
            ModelSimulator nm;
            Int32 nSamples = DEFAULT_NSAMPLES;
            Int32 totalTime = DEFAULT_TOTALTIME;
            Boolean writePotFile = DEFAULT_WRITEPOTFILE;
            Double tolerance = DEFAULT_TOLERANCE;
            Int32 maxTime = DEFAULT_MAXTIME;
            Int32 N = DEFAULT_N;

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
                    else if (parVal[0] == "maxTime")
                        maxTime = Convert.ToInt32(parVal[1]);
                    else if (parVal[0] == "N")
                        N = Convert.ToInt32(parVal[1]);
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
            
            Console.WriteLine("Preparing to measure CPU cycles per timestep...");
            nr = NeuronRegime.Bursting;
            nm = new ModelSimulator(nr, totalTime, tolerance, maxTime, NetworkType.MeanField, N);
            System.Threading.Thread.Sleep(3000);

            Console.WriteLine("Beginning...");
            Console.WriteLine("-");

            if (writePotFile)
            {
                nm.RunModelForData(nm.GLExpMap, totalTime);
                nm.RunModelForData(nm.LIFMap, totalTime);
                nm.RunModelForData(nm.KTzLogMap, totalTime);
                nm.RunModelForData(nm.KTzTanhMap, totalTime);
                nm.RunModelForData(nm.RulkovMap, totalTime);
                nm.RunModelForData(nm.IzhikevichMap, totalTime);
                nm.RunModelForData(nm.HHLeechODE, totalTime);/**/

                // network models
                nm.RunModelForData(nm.NetGLExpMap, totalTime);
                nm.RunModelForData(nm.NetLIFMap, totalTime);
                nm.RunModelForData(nm.NetKTzLogMap, totalTime);
                nm.RunModelForData(nm.NetKTzTanhMap, totalTime);
                nm.RunModelForData(nm.NetRulkovMap, totalTime);
                nm.RunModelForData(nm.NetIzhikevichMap, totalTime);
                nm.RunModelForData(nm.NetHHModelODE, totalTime);
            }

            Console.WriteLine("- Execution cycles/timestep (Cycles/timestep)");

            nm.RunTimeStepTest(nm.KTzTanhMap, nSamples);
            nm.RunTimeStepTest(nm.KTzLogMap, nSamples);
            nm.RunTimeStepTest(nm.GLExpMap, nSamples);
            nm.RunTimeStepTest(nm.LIFMap, nSamples);
            nm.RunTimeStepTest(nm.IzhikevichMap, nSamples);
            nm.RunTimeStepTest(nm.RulkovMap, nSamples);
            nm.RunTimeStepTest(nm.HHLeechODE, nSamples);/**/
            
            Console.WriteLine("--");
            Console.WriteLine("Preparing to measure convergence time...");
            nr = NeuronRegime.Excitable;
            nm = new ModelSimulator(nr, totalTime, tolerance, maxTime, NetworkType.Linear, N);
            System.Threading.Thread.Sleep(3000);

            Console.WriteLine("Beginning...");
            Console.WriteLine("-");

            if (writePotFile)
            {
                nm.RunModelForData(nm.GLExpMap, totalTime);
                nm.RunModelForData(nm.LIFMap, totalTime);
                nm.RunModelForData(nm.KTzLogMap, totalTime);
                nm.RunModelForData(nm.KTzTanhMap, totalTime);
                nm.RunModelForData(nm.RulkovMap, totalTime);
                nm.RunModelForData(nm.IzhikevichMap, totalTime);
                nm.RunModelForData(nm.HHLeechODE, totalTime);

                // network models
                nm.RunModelForData(nm.NetGLExpMap, totalTime);
                nm.RunModelForData(nm.NetLIFMap, totalTime);
                nm.RunModelForData(nm.NetKTzLogMap, totalTime);
                nm.RunModelForData(nm.NetKTzTanhMap, totalTime);
                nm.RunModelForData(nm.NetRulkovMap, totalTime);
                nm.RunModelForData(nm.NetIzhikevichMap, totalTime);
                nm.RunModelForData(nm.NetHHModelODE, totalTime);

                nm.ResetModels(totalTime, tolerance, maxTime);/**/
            }

            nm.RunFPConvergenceTest(nm.KTzTanhMap, nSamples);
            nm.RunFPConvergenceTest(nm.KTzLogMap, nSamples);
            Console.WriteLine("*** WARNING: No FP Convergence time for GLExp model, since it fires stochastically");
            Console.WriteLine("-");
            Console.WriteLine("*** WARNING: No FP Convergence time for LIF model, since it fires constantly");
            Console.WriteLine("-");
            //nm.RunFPConvergenceTest(nm.GLExpMap, nSamples);
            nm.RunFPConvergenceTest(nm.IzhikevichMap, nSamples);
            nm.RunFPConvergenceTest(nm.RulkovMap, nSamples);
            nm.RunFPConvergenceTest(nm.HHStdODE, nSamples);/**/

            Console.WriteLine("--");
            Console.WriteLine("Preparing to measure network time step CPU cycles...");
            Console.WriteLine("-");
            Console.WriteLine("Linear Network (N = {0}, signal propagation)", N);
            nr = NeuronRegime.Excitable;
            nm = new ModelSimulator(nr, totalTime, tolerance, maxTime, NetworkType.Linear, N);
            System.Threading.Thread.Sleep(3000);

            nm.RunTimeStepTest(nm.NetGLExpMap, nSamples);
            nm.RunTimeStepTest(nm.NetLIFMap, nSamples);
            nm.RunTimeStepTest(nm.NetKTzLogMap, nSamples);
            nm.RunTimeStepTest(nm.NetKTzTanhMap, nSamples);
            nm.RunTimeStepTest(nm.NetRulkovMap, nSamples);
            nm.RunTimeStepTest(nm.NetIzhikevichMap, nSamples);
            nm.RunTimeStepTest(nm.NetHHModelODE, nSamples);

            Console.WriteLine("-");
            Console.WriteLine("Mean Field Network (N = {0}, synchronization)", N);
            nr = NeuronRegime.Bursting;
            nm = new ModelSimulator(nr, totalTime, tolerance, maxTime, NetworkType.MeanField, N);
            System.Threading.Thread.Sleep(3000);

            nm.RunTimeStepTest(nm.NetGLExpMap, nSamples);
            nm.RunTimeStepTest(nm.NetLIFMap, nSamples);
            nm.RunTimeStepTest(nm.NetKTzLogMap, nSamples);
            nm.RunTimeStepTest(nm.NetKTzTanhMap, nSamples);
            nm.RunTimeStepTest(nm.NetRulkovMap, nSamples);
            nm.RunTimeStepTest(nm.NetIzhikevichMap, nSamples);
            nm.RunTimeStepTest(nm.NetHHModelODE, nSamples);
            
            
#if DEBUG
            //Console.ReadKey();
#endif
        }
    }

    public class Statistics
    {
        public Double StdDev { get; private set; }
        public Double RndErr { get; private set; }
        public Double Average { get; private set; }
        public Double[] Data { get; private set; }
        
        public Statistics()
        {
            this.StdDev = 0.0D;
            this.RndErr = 0.0D;
            this.Average = 0.0D;
            this.Data = new Double[0];
        }

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
        public NetworkType netType { get; private set; }
        public LIFModel LIFMap { get; private set; }
        public GLExpModel GLExpMap { get; private set; }
        public KTzLogModel KTzLogMap { get; private set; }
        public KTzTanhModel KTzTanhMap { get; private set; }
        public RulkovModel RulkovMap { get; private set; }
        public IzhikevichModel IzhikevichMap { get; private set; }
        public HHLeechModel HHLeechODE { get; private set; }
        public HHStdModel HHStdODE { get; private set; }
        public NetworkModel NetLIFMap { get; private set; }
        public NetworkModel NetGLExpMap { get; private set; }
        public NetworkModel NetKTzLogMap { get; private set; }
        public NetworkModel NetKTzTanhMap { get; private set; }
        public NetworkModel NetRulkovMap { get; private set; }
        public NetworkModel NetIzhikevichMap { get; private set; }
        public NetworkModel NetHHModelODE { get; private set; }
        public Int32 totalTime { get; private set; }
        public Double tolerance { get; private set; }
        public Int32 maxTime { get; private set; }

        private List<Int32> tsForFixedPointPerModel;
        private Dictionary<String,Statistics> tsForFPStat; // one entry for each model

        public ModelSimulator(NeuronRegime nr, Int32 totalTime, Double tolerance, Int32 maxTime, NetworkType nt, Int32 N)
        {
            this.neuronRegime = nr;
            this.GLExpMap = new GLExpModel(nr, totalTime);
            this.LIFMap = new LIFModel(nr, totalTime);
            this.KTzLogMap = new KTzLogModel(nr, totalTime);
            this.KTzTanhMap = new KTzTanhModel(nr, totalTime);
            this.RulkovMap = new RulkovModel(nr, totalTime);
            this.IzhikevichMap = new IzhikevichModel(nr, totalTime);
            this.HHLeechODE = new HHLeechModel(nr, totalTime);
            this.HHStdODE = new HHStdModel(nr, totalTime);
            if (nr == NeuronRegime.Excitable)
            {
                this.NetLIFMap = new NetworkModel(NeuronType.LIF, nr, totalTime, nt, N, 0.1);
                this.NetGLExpMap = new NetworkModel(NeuronType.GLExp, nr, totalTime, nt, N, 0.1);
                this.NetKTzLogMap = new NetworkModel(NeuronType.KTzLog, nr, totalTime, nt, N, 0.1);
                this.NetKTzTanhMap = new NetworkModel(NeuronType.KTzTanh, nr, totalTime, nt, N, 0.04);
                this.NetRulkovMap = new NetworkModel(NeuronType.Rulkov, nr, totalTime, nt, N, 0.08);
                this.NetIzhikevichMap = new NetworkModel(NeuronType.Izhikevich, nr, totalTime, nt, N, 0.1);
                this.NetHHModelODE = new NetworkModel(NeuronType.HodgkinHuxley, nr, totalTime, nt, N, 0.05);
            }
            else if (nr == NeuronRegime.Bursting)
            {
                this.NetLIFMap = new NetworkModel(NeuronType.LIF, nr, totalTime, nt, N, 0.1);
                this.NetGLExpMap = new NetworkModel(NeuronType.GLExp, nr, totalTime, nt, N, 0.1);
                this.NetKTzLogMap = new NetworkModel(NeuronType.KTzLog, nr, totalTime, nt, N, 0.0);
                this.NetKTzTanhMap = new NetworkModel(NeuronType.KTzTanh, nr, totalTime, nt, N, 0.04);
                this.NetRulkovMap = new NetworkModel(NeuronType.Rulkov, nr, totalTime, nt, N, 0.08);
                this.NetIzhikevichMap = new NetworkModel(NeuronType.Izhikevich, nr, totalTime, nt, N, 0.1);
                this.NetHHModelODE = new NetworkModel(NeuronType.HodgkinHuxley, nr, totalTime, nt, N, 1.0e-10);
            }
            this.ResetModels(totalTime, tolerance, maxTime);
            this.tsForFixedPointPerModel = new List<Int32>();
            this.tsForFPStat = new Dictionary<String, Statistics>(5);
            this.tsForFPStat.Add(this.LIFMap.ToString(), new Statistics());
            this.tsForFPStat.Add(this.GLExpMap.ToString(), new Statistics());
            this.tsForFPStat.Add(this.KTzLogMap.ToString(), new Statistics());
            this.tsForFPStat.Add(this.KTzTanhMap.ToString(), new Statistics());
            this.tsForFPStat.Add(this.IzhikevichMap.ToString(), new Statistics());
            this.tsForFPStat.Add(this.RulkovMap.ToString(), new Statistics());
            this.tsForFPStat.Add(this.HHStdODE.ToString(), new Statistics());
            this.tsForFPStat.Add(this.NetLIFMap.ToString(), new Statistics());
            this.tsForFPStat.Add(this.NetGLExpMap.ToString(), new Statistics());
            this.tsForFPStat.Add(this.NetKTzLogMap.ToString(), new Statistics());
            this.tsForFPStat.Add(this.NetKTzTanhMap.ToString(), new Statistics());
            this.tsForFPStat.Add(this.NetRulkovMap.ToString(), new Statistics());
            this.tsForFPStat.Add(this.NetIzhikevichMap.ToString(), new Statistics());
            this.tsForFPStat.Add(this.NetHHModelODE.ToString(), new Statistics());
        }

        private void RunTransient(IModel model)
        {
            Int32 t = 0, tTotal = (Int32)((Double)model.transient / model.dt) + 1;
            while (t < tTotal)
            {
                model.TimeStep();
                t++;
            }
        }

        public void RunFPConvergenceTest(IModel model, Int32 nSamples)
        {
            System.Threading.Thread.Sleep(1000);
            Console.WriteLine("* {0} Model:", model.ToString());
            List<PerformanceStatus> pStat = this.TestFPConvergenceModel(model, nSamples);
            Statistics[] executionStat = new Statistics[5];
            executionStat[0] = new Statistics(pStat.Select(e => (Double)e.CPUCycles).ToArray<Double>());
            executionStat[1] = new Statistics(pStat.Select(e => (Double)e.GCCount1).ToArray<Double>());
            executionStat[2] = new Statistics(pStat.Select(e => (Double)e.GCCount2).ToArray<Double>());
            executionStat[3] = new Statistics(pStat.Select(e => (Double)e.GCCount3).ToArray<Double>());
            Console.WriteLine("  TimeStep amount: {0} +/- {1}", this.tsForFPStat[model.ToString()].Average, this.tsForFPStat[model.ToString()].StdDev);
            Console.WriteLine("    CPU => {0} +/- {1} cycles to converge", executionStat[0].Average, executionStat[0].StdDev);
            Console.WriteLine("          * GC_0 = {0} +/- {1}\tGC_1 = {2} +/- {3}\tGC_2 = {4} +/- {5}", executionStat[1].Average, executionStat[1].StdDev,
                executionStat[2].Average, executionStat[2].StdDev, executionStat[3].Average, executionStat[3].StdDev);
            /*Console.WriteLine("* {0} Model:", model.ToString());
            Statistics[] executionStat = this.TestFPConvergenceModel(model, nSamples);
            Console.WriteLine("  - Real world convergence time (ns) = {0} +/- {1}", executionStat[0].Average * 100.0D, executionStat[0].StdDev * 100.0D);
            Console.WriteLine("  - Model convergence time (ts)      = {0} +/- {1}", executionStat[1].Average, executionStat[1].StdDev);*/
            Console.WriteLine("-");
        }

        private List<PerformanceStatus> TestFPConvergenceModel(IModel model, Int32 nSamples)//, Int32 maxTime, Double tolerance)//, Double Iext)//private Statistics[] TestFPConvergenceModel(IModel model, Int32 nSamples)//, Int32 maxTime, Double tolerance)//, Double Iext)
        {
            /*
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
                model.Reset(this.neuronRegime, mTime);
                Double xAnt = model.GetV();
                sw.Start();
                while (t < mTime)
                {
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
            return res;*/
            Int32 i;//, t;
            List<PerformanceStatus> perfStat = new List<PerformanceStatus>(nSamples);
            //this.tsForFixedPointPerModel = new List<Int32>(nSamples); // this list is filled by the following function call:
            for (i = 0; i < nSamples; i++)
            {
                model.Reset(this.neuronRegime, (Int32)Math.Ceiling((double)this.maxTime / model.dt));
                CodeTimer c = new CodeTimer(this.RunModelFixedPoint, model);
                perfStat.Add(c.Time());
            }
            this.tsForFPStat[model.ToString()] = new Statistics(this.tsForFixedPointPerModel.Select(e => (Double)e).ToArray<Double>());
            return perfStat;
        }

        public void RunModelFixedPoint(IModel model)
        {
            this.tsForFixedPointPerModel = new List<Int32>(); // this list is filled by the following function call:
            Int32 tMax = (Int32)Math.Ceiling((double)this.maxTime / model.dt);
            Int32 t = tMax;
            Double xAnt = model.GetV();
            while (t-- > 0)
            {
                model.TimeStep();
                if (Math.Abs(model.GetV() - xAnt) < this.tolerance)
                {
                    this.tsForFixedPointPerModel.Add(tMax - t);
                    return;
                }
                xAnt = model.GetV();
            }
        }

        public void RunTimeStepTest(IModel model, Int32 nSamples)
        {
            System.Threading.Thread.Sleep(1000);
            Console.WriteLine("* {0} Model:", model.ToString());
            List<PerformanceStatus> pStat = this.TestTimeStepModel(model, nSamples);
            Statistics[] executionStat = new Statistics[4];
            executionStat[0] = new Statistics(pStat.Select(e => (Double)e.CPUCycles / Math.Ceiling(model.ts_per_ms * (Double)this.totalTime)).ToArray<Double>());
            executionStat[1] = new Statistics(pStat.Select(e => (Double)e.GCCount1).ToArray<Double>());
            executionStat[2] = new Statistics(pStat.Select(e => (Double)e.GCCount2).ToArray<Double>());
            executionStat[3] = new Statistics(pStat.Select(e => (Double)e.GCCount3).ToArray<Double>());
            Console.WriteLine("  CPU => {0} +/- {1} cycles/timestep", executionStat[0].Average, executionStat[0].StdDev);
            Console.WriteLine("        * GC_0 = {0} +/- {1}\tGC_1 = {2} +/- {3}\tGC_2 = {4} +/- {5}", executionStat[1].Average, executionStat[1].StdDev,
                executionStat[2].Average, executionStat[2].StdDev, executionStat[3].Average, executionStat[3].StdDev);
            Console.WriteLine("-");
        }

        private List<PerformanceStatus> TestTimeStepModel(IModel model, Int32 nSamples)
        {
            Int32 i;//, t;
            List<PerformanceStatus> perfStat = new List<PerformanceStatus>(nSamples);
            for (i = 0; i < nSamples; i++)
            {
                CodeTimer c = new CodeTimer(this.RunModel, model);
                perfStat.Add(c.Time());
            }
            return perfStat;
        }

        public void RunModel(IModel model)
        {
            Int32 t = (Int32)Math.Ceiling(model.ts_per_ms * (Double)this.totalTime);
            while (t-- > 0)
                model.TimeStep();
        }

        public void RunModelForData(IModel model, Int32 totalTime)
        {
            Double[][] sData;
            if (model.isNetwork)
            {
                sData = this.RunModelForDataNetwork(model, totalTime);
            }
            else
            {
                sData = this.RunModelForDataNeuron(model, totalTime);
            }
            String suffix = (this.neuronRegime == NeuronRegime.Bursting ? "bst" : "exc");
            OutputFile of = new OutputFile(model.ToString() + "_" + suffix + ".dat");
            of.WriteData("0.00000000e+000", "\t", "#t\tV", true, sData);
            of.Close();
        }
        public Double[][] RunModelForDataNeuron(IModel model, Int32 totalTime)
        {
            Int32 mTotalTime = (Int32)((Double)model.ts_per_ms * (Double)totalTime);
            //Double[] tData = new Double[mTotalTime];
            //Double[] xData = new Double[mTotalTime];
            Double[][] sData = new Double[mTotalTime][];


            Int32 t = 0;
            while (t < mTotalTime)
            {
                model.TimeStep();
                sData[t] = new Double[2] { (Double)t * model.dt, model.GetV() };
                //tData[t] = (Double)t * model.dt;
                //xData[t] = model.GetV();
                t++;
            }

            return sData;
        }
        public Double[][] RunModelForDataNetwork(IModel model, Int32 totalTime)
        {
            Int32 mTotalTime = (Int32)((Double)model.ts_per_ms * (Double)totalTime);
            //Double[] tData = new Double[mTotalTime];
            //Double[][] xData = new Double[mTotalTime][];
            Double[] xData;
            Double[][] sData = new Double[mTotalTime][];
            Int32 t = 0, i;
            while (t < mTotalTime)
            {
                sData[t] = new Double[model.N + 1];
                t++;
            }

            t = 0;
            while (t < mTotalTime)
            {
                model.TimeStep();
                //tData[t] = (Double)t * model.dt;
                //xData[t] = model.GetVNet();
                xData = model.GetVNet();
                sData[t][0] = (Double)t * model.dt;
                i = 1;
                while (i <= model.N)
                {
                    sData[t][i] = xData[i - 1];
                    i++;
                }
                t++;
            }

            return sData;
        }

        public void ResetModels(Int32 totalTime, Double tolerance, Int32 maxTime)
        {
            this.totalTime = totalTime;
            this.tolerance = tolerance;
            this.maxTime = maxTime;
            this.GLExpMap.Reset(this.neuronRegime, totalTime);
            this.LIFMap.Reset(this.neuronRegime, totalTime);
            this.KTzLogMap.Reset(this.neuronRegime, totalTime);
            this.KTzTanhMap.Reset(this.neuronRegime, totalTime);
            this.RulkovMap.Reset(this.neuronRegime, totalTime);
            this.IzhikevichMap.Reset(this.neuronRegime, totalTime);
            this.HHLeechODE.Reset(this.neuronRegime, totalTime);
            this.HHStdODE.Reset(this.neuronRegime, totalTime);
            this.NetLIFMap.Reset(this.neuronRegime, totalTime);
            this.NetGLExpMap.Reset(this.neuronRegime, totalTime);
            this.NetKTzLogMap.Reset(this.neuronRegime, totalTime);
            this.NetKTzTanhMap.Reset(this.neuronRegime, totalTime);
            this.NetRulkovMap.Reset(this.neuronRegime, totalTime);
            this.NetIzhikevichMap.Reset(this.neuronRegime, totalTime);
            this.NetHHModelODE.Reset(this.neuronRegime, totalTime);

            if (this.neuronRegime == NeuronRegime.Bursting)
            {
                this.RunTransient(this.KTzTanhMap);
                this.RunTransient(this.KTzLogMap);
                this.RunTransient(this.GLExpMap);
                this.RunTransient(this.LIFMap);
                this.RunTransient(this.RulkovMap);
                this.RunTransient(this.IzhikevichMap);
                this.RunTransient(this.HHLeechODE);
                this.RunTransient(this.HHStdODE);
            }
        }
    }
}
