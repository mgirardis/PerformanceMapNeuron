using System;

namespace NeuronPerformance.Models
{
    public enum NeuronRegime
    {
        Excitable,
        Bursting
    }
    public enum NeuronType
    {
        HodgkinHuxley,
        KTzTanh,
        KTzLog,
        Izhikevich,
        Rulkov,
        GLExp,
        LIF
    }
    public static class NeuronFactory
    {
        public static Neuron Create(NeuronType nt, NeuronRegime nr, Int32 totalTime)
        {
            if (nt == NeuronType.HodgkinHuxley)
            {
                if (nr == NeuronRegime.Bursting)
                {
                    return new HHLeechModel(nr, totalTime);
                }
                else if (nr == NeuronRegime.Excitable)
                {
                    return new HHStdModel(nr, totalTime);
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Unrecognized NeuronRegime");
                }
            }
            else if (nt == NeuronType.Rulkov)
            {
                return new RulkovModel(nr, totalTime);
            }
            else if (nt == NeuronType.Izhikevich)
            {
                return new IzhikevichModel(nr, totalTime);
            }
            else if (nt == NeuronType.KTzTanh)
            {
                return new KTzTanhModel(nr, totalTime);
            }
            else if (nt == NeuronType.KTzLog)
            {
                return new KTzLogModel(nr, totalTime);
            }
            else if (nt == NeuronType.GLExp)
            {
                return new GLExpModel(nr, totalTime);
            }
            else if (nt == NeuronType.LIF)
            {
                return new LIFModel(nr, totalTime);
            }
            else
            {
                throw new ArgumentOutOfRangeException("Unrecognized NeuronType");
            }
        }
    }

    public class LIFModel : Neuron
    {
        // KTz Log parameters
        private Double VR = 10.0;
        private Double VT = 20.0;
        private Double tau = 20.0;
        private Double Iext = 30.0;

        // KTz Log variables
        private Double V = 0.0;

        /*public Double dt { get; private set; } // the step of the integration
        public Double ts_per_ms { get; private set; }
        public Int32 transient { get; private set; }*/

        public LIFModel(NeuronRegime nr, Int32 totalTime)
        {
            this.Reset(nr, totalTime);
        }

        public override void Reset(NeuronRegime nr, Int32 totalTime)
        {
            this.ts_per_ms = 100.0 / 10.0;
            this.transient = 1000; // in ts
            this.dt = 0.1;
            if (nr == NeuronRegime.Bursting)
            {
                this.SetBurstingParam();
            }
            else
            {
                this.SetExcitableParam();
            }
        }

        public override void SetBurstingParam()
        {
            this.tau = 20.0;
            this.Iext = 30.0;
            this.VR = 10.0;
            this.VT = 20.0;
            this.V = 0.0;
        }

        public override void SetExcitableParam()
        {
            this.tau = 20.0;
            this.Iext = 30.0;
            this.VR = 10.0;
            this.VT = 20.0;
            this.V = 0.0;
        }

        public override void SetIC(Double[] ic)
        {
            this.V = ic[0];
        }

        public override Double[] ResetToFP()
        {
            this.V = 0.0;
            return new Double[3] { this.V, 0.0, 0.0 };
        }

        /// <summary>
        /// KTz timestep
        /// </summary>
        /// <param name="Iext">external input</param>
        //public void TimeStep(Double Iext)
        public override void TimeStep()
        {
            if (this.V > this.VT)
            {
                this.V = this.VR;
            }
            else
            {
                this.V = this.V + this.dt * (-this.V / this.tau + this.Iext);
            }
        }

        public override void TimeStepNet()
        {
            this.SumInputSignal();
            if (this.V > this.VT)
            {
                this.V = this.VR;
            }
            else
            {
                this.V = this.V + this.dt * (-this.V / this.tau + this.Iext);
            }
        }

        public override Double GetV()
        {
            return this.V;
        }

        public override String ToString()
        {
            return "LIFModel";
        }
    }

    public class GLExpModel : Neuron
    {
        // KTz Log parameters
        private Double A = 1.0/27.07;
        private Double VM = 20.0;
        private Double VS = 1.20;
        private Double VR = 10.0;
        private Double tau = 20.0;
        private Double Iext = 0;

        // KTz Log variables
        private Double V = 0.0;
        private Boolean X = false;
        System.Random rand;

        /*public Double dt { get; private set; } // the step of the integration
        public Double ts_per_ms { get; private set; }
        public Int32 transient { get; private set; }*/

        public GLExpModel(NeuronRegime nr, Int32 totalTime)
        {
            this.Reset(nr, totalTime);
            this.rand = new System.Random();
        }

        public override void Reset(NeuronRegime nr, Int32 totalTime)
        {
            this.ts_per_ms = 100.0 / 10.0;
            this.transient = 1000; // in ts
            this.dt = 0.1;
            if (nr == NeuronRegime.Bursting)
            {
                this.SetBurstingParam();
            }
            else
            {
                this.SetExcitableParam();
            }
        }

        public override void SetBurstingParam()
        {
            this.A = 1.0 / 27.07;
            this.VM = 20.0;
            this.VS = 1.20;
            this.tau = 20.0;
            this.Iext = 30.0;
            this.VR = 10.0;
            this.V = 0.0;
            this.X = false;
        }

        public override void SetExcitableParam()
        {
            this.A = 1.0 / 27.07;
            this.VM = 20.0;
            this.VS = 1.20;
            this.tau = 20.0;
            this.Iext = 30.0;
            this.VR = 10.0;
            this.V = 0.0;
            this.X = false;
        }

        public override void SetIC(Double[] ic)
        {
            this.V = ic[0];
            this.X = false;
        }

        public override Double[] ResetToFP()
        {
            this.V = 0.0;
            this.X = false;
            return new Double[3] { this.V, 0.0, 0.0 };
        }

        /// <summary>
        /// KTz timestep
        /// </summary>
        /// <param name="Iext">external input</param>
        //public void TimeStep(Double Iext)
        public override void TimeStep()
        {
            if (this.X)
            {
                this.V = this.VR;
            }
            else
            {
                this.V = this.V + this.dt * (-this.V / this.tau + this.Iext);
            }
            this.X = this.rand.NextDouble() < this.Phi();
        }

        public override void TimeStepNet()
        {
            this.SumInputSignal();
            if (this.X)
            {
                this.V = this.VR;
            }
            else
            {
                this.V = this.V + this.dt * (-this.V / this.tau + this.Iext + this.Isyn);
            }
            this.X = this.rand.NextDouble() < this.Phi();
        }

        private Double Phi()
        {
            return this.A * Math.Exp((this.V - this.VM) / this.VS);
        }

        public override Double GetV()
        {
            return this.V;
        }

        public override String ToString()
        {
            return "GLExpModel";
        }
    }

    public class KTzLogModel : Neuron
    {
        // KTz Log parameters
        private Double K = 0.6;
        private Double T = 0.3;
        private Double d = 0.001;
        private Double l = 0.003;
        private Double xR = -0.3;
        private Double Iext = 0.2;

        // KTz Log variables
        private Double x = -0.2;
        private Double xAux = -0.2;
        private Double y = -0.2;
        private Double z = -0.2;

        /*public Double dt { get; private set; } // the step of the integration
        public Double ts_per_ms { get; private set; }
        public Int32 transient { get; private set; }*/

        public KTzLogModel(NeuronRegime nr, Int32 totalTime)
        {
            this.Reset(nr, totalTime);
        }

        public override void Reset(NeuronRegime nr, Int32 totalTime)
        {
            this.ts_per_ms = 100.0 / 10.0;
            this.transient = 1000; // in ts
            this.dt = 1.0;
            if (nr == NeuronRegime.Bursting)
            {
                this.SetBurstingParam();
            }
            else
            {
                this.SetExcitableParam();
            }
        }

        public override void SetBurstingParam()
        {
            this.K = 0.6;
            this.T = 0.3;
            this.d = 0.001;
            this.l = 0.003;
            this.xR = -0.3;
            this.Iext = 0.2;

            // KTz Tanh variables
            this.x = -0.2;
            this.xAux = -0.2;
            this.y = -0.2;
            this.z = -0.2;
        }

        public override void SetExcitableParam()
        {
            this.K = 0.6;
            this.T = 0.32;
            this.d = 0.05;
            this.l = 0.01;
            this.xR = -0.5;
            this.Iext = 0.0;

            // KTz Tanh variables
            this.x = -0.1;
            this.xAux = -0.1;
            this.y = -0.3458236433584459;
            this.z = -0.0308352713283108;
        }

        public override void SetIC(Double[] ic)
        {
            this.x = ic[0];
            this.xAux = ic[0];
            this.y = ic[1];
            this.z = ic[2];
        }

        public override Double[] ResetToFP()
        {
            this.x = -0.3458236433584459;
            this.xAux = this.x;
            this.y = this.x;
            this.z = -0.0308352713283108;
            return new Double[3] { this.x, this.y, this.z };
        }

        /// <summary>
        /// KTz timestep
        /// </summary>
        /// <param name="Iext">external input</param>
        //public void TimeStep(Double Iext)
        public override void TimeStep()
        {
            this.xAux = this.x;
            this.x = this.KTzLogFunction((this.xAux - (this.K * this.y) + this.z + this.Iext) / this.T);
            this.y = this.xAux;
            this.z = (1.0D - this.d) * this.z - this.l * (this.xAux - this.xR);
        }

        public override void TimeStepNet()
        {
            this.SumInputSignal();
            this.xAux = this.x;
            this.x = this.KTzLogFunction((this.xAux - (this.K * this.y) + this.z + this.Isyn + this.Iext) / this.T);
            this.y = this.xAux;
            this.z = (1.0D - this.d) * this.z - this.l * (this.xAux - this.xR);
        }

        private Double KTzLogFunction(Double x)
        {
            //return x / (1.0D + (x > 0 ? x : -x));
            if (x > 0)
            {
                return x / (1.0D + x);
            }
            else
            {
                return x / (1.0D - x);
            }
        }

        public override Double GetV()
        {
            return this.x;
        }

        public override String ToString()
        {
            return "KTzLogModel";
        }
    }

    public class KTzTanhModel : Neuron
    {
        // KTz Tanh parameters
        private Double K = 0.6;
        private Double T = 0.35;
        private Double d = 0.001;
        private Double l = 0.001;
        private Double xR = -0.5;
        private Double Iext = 0.0;

        // KTz Tanh variables
        private Double x = 0;
        private Double xAux = 0;
        private Double y = 0;
        private Double z = 0;

        /*public Double dt { get; private set; } // the step of the integration
        public Double ts_per_ms { get; private set; }
        public Int32 transient { get; private set; }*/

        public KTzTanhModel(NeuronRegime nr, Int32 totalTime)
        {
            this.Reset(nr, totalTime);
        }

        public override void Reset(NeuronRegime nr, Int32 totalTime)
        {
            this.ts_per_ms = 100.0 / 10.0;
            this.transient = 1000; // in ts
            this.dt = 1.0;
            if (nr == NeuronRegime.Bursting)
            {
                this.SetBurstingParam();
            }
            else
            {
                this.SetExcitableParam();
            }
        }

        public override void SetBurstingParam()
        {
            this.K = 0.6;
            this.T = 0.35;
            this.d = 0.001;
            this.l = 0.001;
            this.xR = -0.5;
            this.Iext = 0.0;

            // KTz Tanh variables
            this.x = 0;
            this.xAux = 0;
            this.y = 0;
            this.z = 0;
        }

        public override void SetExcitableParam()
        {
            this.K = 0.6;
            this.T = 0.35;
            this.d = 0.007;
            this.l = 0.004;
            this.xR = -0.7;
            this.Iext = 0.0;

            // KTz Tanh variables
            this.x = -0.5;
            this.xAux = -0.5;
            this.y = -0.6717116617084296;
            this.z = -0.0161647647380404;
        }

        public override void SetIC(Double[] ic)
        {
            this.x = ic[0];
            this.xAux = ic[0];
            this.y = ic[1];
            this.z = ic[2];
        }

        public override Double[] ResetToFP()
        {
            this.x = -0.6717116617084296;
            this.xAux = this.x;
            this.y = this.x;
            this.z = -0.0161647647380404;
            return new Double[3] { this.x, this.y, this.z };
        }

        /// <summary>
        /// KTz timestep
        /// </summary>
        /// <param name="Iext">external input</param>
        //public void TimeStep(Double Iext)
        public override void TimeStep()
        {
            this.xAux = this.x;
            this.x = Math.Tanh((this.xAux - (this.K * this.y) + this.z + this.Iext) / this.T);
            this.y = this.xAux;
            this.z = (1.0D - this.d) * this.z - this.l * (this.xAux - this.xR);
        }

        public override void TimeStepNet()
        {
            this.SumInputSignal();
            this.xAux = this.x;
            this.x = Math.Tanh((this.xAux - (this.K * this.y) + this.z + this.Isyn + this.Iext) / this.T);
            this.y = this.xAux;
            this.z = (1.0D - this.d) * this.z - this.l * (this.xAux - this.xR);
        }

        public override Double GetV()
        {
            return this.x;
        }

        public override String ToString()
        {
            return "KTzTanhModel";
        }
    }

    public class IzhikevichModel : Neuron
    {
        // Izhikevich model
        private Double a = 0.02;
        private Double b = 0.25;
        private Double c = -57.0D;
        private Double d = 0.0;
        private Double vReset = 30.0;
        private Double Iext = 2.0D;

        // Izhikevich variables
        private Double v = 0.1;
        private Double vAux = 0.1;
        private Double u = 0.1;

        /*public Double dt { get; private set; } // the step of the integration
        public Double ts_per_ms { get; private set; }
        public Int32 transient { get; private set; }*/

        public IzhikevichModel(NeuronRegime nr, Int32 totalTime)
        {
            this.Reset(nr, totalTime);
        }

        public override void Reset(NeuronRegime nr, Int32 totalTime)
        {
            this.ts_per_ms = 100.0 / 20.0;
            this.transient = 1000; // in ts
            this.dt = 1.0;
            if (nr == NeuronRegime.Bursting)
            {
                this.SetBurstingParam();
            }
            else
            {
                this.SetExcitableParam();
            }
        }

        public override void SetBurstingParam()
        {
            this.a = 0.02;
            this.b = 0.25;
            this.c = -57.0D;
            this.d = 0.0;
            this.vReset = 30.0;
            this.Iext = 2.0D;

            // Izhikevich variables
            this.v = 0.1;
            this.vAux = 0.1;
            this.u = 0.1;
        }

        public override void SetExcitableParam()
        {
            this.a = 0.02;
            this.b = 0.25;
            this.c = -62.0D;
            this.d = 0.0;
            this.vReset = 30.0;
            this.Iext = 0.6D;

            // Izhikevich variables
            this.v = -56.0D; // FP: -62.5984492395571 -15.6496123098892
            this.vAux = -56.0D;
            this.u = -15.649612309889193;
        }

        public override void SetIC(Double[] ic)
        {
            this.v = ic[0];
            this.vAux = ic[0];
            this.u = ic[1];
        }

        public override Double[] ResetToFP()
        {
            this.v = -62.5984492395571D;
            this.vAux = this.v;
            this.u = -15.649612309889193D;
            return new Double[2] { this.v, this.u };
        }

        /// <summary>
        /// Izhikevich timestep
        /// </summary>
        /// <param name="Iext">external input</param>
        //public void TimeStep(Double Iext)
        public override void TimeStep()
        {
            if (this.v < this.vReset)
            {
                this.vAux = this.v;
                this.v = 0.04D * this.v * this.v + 6.0D * this.v + 140.0D - this.u + this.Iext;
                this.u = this.u + this.a * (this.b * this.vAux - this.u);
            }
            else
            {
                this.v = this.c;
                this.u = this.u + this.d;
            }
        }

        public override void TimeStepNet()
        {
            this.SumInputSignal();
            if (this.v < this.vReset)
            {
                this.vAux = this.v;
                this.v = 0.04D * this.v * this.v + 6.0D * this.v + 140.0D - this.u + this.Iext + this.Isyn;
                this.u = this.u + this.a * (this.b * this.vAux - this.u);
            }
            else
            {
                this.v = this.c;
                this.u = this.u + this.d;
            }
        }

        public override Double GetV()
        {
            return this.v;
        }

        public override String ToString()
        {
            return "IzhikevichModel";
        }
    }

    public class RulkovModel : Neuron
    {
        // Rulkov parameters
        private Double a = 6.0;
        private Double s = -1.1;
        private Double m = 0.001;

        // Rulkov variables
        private Double x = -0.1;
        private Double xAux = -0.1;
        private Double y = -0.1;

        private Double Iext = 0.0D;
        
        /*public Double dt { get; private set; } // the step of the integration
        public Double ts_per_ms { get; private set; }
        public Int32 transient { get; private set; }*/

        public RulkovModel(NeuronRegime nr, Int32 totalTime)
        {
            this.Reset(nr, totalTime);
        }

        public override void Reset(NeuronRegime nr, Int32 totalTime)
        {
            this.ts_per_ms = 100.0 / 10.0;
            this.transient = 2000; // in ts
            this.dt = 1.0;
            if (nr == NeuronRegime.Bursting)
            {
                this.SetBurstingParam();
            }
            else
            {
                this.SetExcitableParam();
            }
        }

        public override void SetBurstingParam()
        {
            this.a = 6.0;
            this.s = -1.1;
            this.m = 0.001;

            // Rulkov variables
            this.x = -0.1;
            this.xAux = -0.1;
            this.y = -0.1;

            this.Iext = 0.0D;
        }

        public override void SetExcitableParam()
        {
            this.a = 2.5;
            this.s = -0.6;
            this.m = 0.001;

            // Rulkov variables
            this.x = -0.4D; // FP: -0.600000000000  -2.1625
            this.xAux = -0.4D;
            this.y = -2.1625D;

            this.Iext = 0.0D;
        }

        public override void SetIC(Double[] ic)
        {
            this.x = ic[0];
            this.xAux = ic[0];
            this.y = ic[1];
        }

        public override Double[] ResetToFP()
        {
            this.x = -0.6D;
            this.xAux = this.x;
            this.y = -2.1625D;
            return new Double[2] { this.x, this.y };
        }

        /// <summary>
        /// Rulkov timestep
        /// </summary>
        /// <param name="Iext">external input</param>
        //public void TimeStep(Double Iext)
        public override void TimeStep()
        {
            this.xAux = this.x;
            this.x = this.RulkovFunction(this.x, this.y + this.Iext);
            this.y = this.y - this.m * (this.xAux - this.s);
        }

        public override void TimeStepNet()
        {
            this.SumInputSignal();
            this.xAux = this.x;
            this.x = this.RulkovFunction(this.x, this.y + this.Isyn + this.Iext);
            this.y = this.y - this.m * (this.xAux - this.s);
        }

        private Double RulkovFunction(Double x, Double y)
        {
            if (x <= 0)
            {
                return y + this.a / (1.0D - x);
            }
            else if (x < (this.a + y))
            {
                return this.a + y;
            }
            else
            {
                return -1.0D;
            }
        }

        public override Double GetV()
        {
            return this.x;
        }

        public override String ToString()
        {
            return "RulkovModel";
        }
    }

    public class HHStdModel : Neuron
    {
        // HH parameters
        private Double gLNa = 0.0265; //e-6Siemens
        private Double gLK = 0.07; //e-6Siemens
        private Double gLCl = 0.1; //e-6Siemens
        private Double ELCl = -57.2; //e-3Volts
        private Double gK = 36; //e-6Siemens
        private Double EK = -72.1; //e-3Volts
        private Double gNa = 120; //e-6Siemens
        private Double ENa = 52.4; //e-3Volts
        private Double C = 1; //e-9Faraday
        private Double gL;
        private Double EL;
        private Double Iext = 0.0D;

        // HH variables
        private Double h = 0.514071155409594D;// initial condition
        private Double m = 0.069236269218388D;//
        private Double n =  0.353721170200369D;
        private Double V = -59.0D;//initial condition -- FP_V = -62.698490240702419
        private Double[] initState;
        private Double[] currState;
        private Double[] c1;
        private Double[] c2;
        private Double[] c3;
        private Double[] c4;
        private Double[] dydt;

        /*public Double dt { get; private set; } // the step of the integration
        public Double ts_per_ms { get; private set; }
        public Int32 transient { get; private set; }*/

        public HHStdModel(NeuronRegime nr, Int32 totalTime)
        {
            this.Reset(nr, totalTime);
        }

        public override void Reset(NeuronRegime nr, Int32 totalTime)
        {
            this.initState = new Double[4];
            this.currState = new Double[4];
            this.c1 = new Double[4];
            this.c2 = new Double[4];
            this.c3 = new Double[4];
            this.c4 = new Double[4];
            this.dydt = new Double[4];
            this.dt = 0.01;
            this.ts_per_ms = 1 / this.dt;
            this.transient = 100; // in ms
            if (nr == NeuronRegime.Bursting)
            {
                this.SetBurstingParam();
            }
            else
            {
                this.SetExcitableParam();
            }
        }

        public override void SetBurstingParam()
        {
            this.SetExcitableParam();
            this.Iext = 3.7161D; //e-9 Ampere
        }

        public override void SetExcitableParam()
        {
            this.gLNa = 0.0265; //e-6Siemens
            this.gLK = 0.07; //e-6Siemens
            this.gLCl = 0.1; //e-6Siemens
            this.ELCl = -57.2; //e-3Volts
            this.gK = 36; //e-6Siemens
            this.EK = -72.1; //e-3Volts
            this.gNa = 120; //e-6Siemens
            this.ENa = 52.4; //e-3Volts
            this.C = 1; //e-9Faraday
            this.Iext = 0.0D;
            this.h = 0.514071155409594D;// initial condition
            this.m = 0.069236269218388D;//
            this.n =  0.353721170200369D;
            this.V = -59.0D;//initial condition -- FP_V = -62.698490240702419
            this.gL = gLK + gLNa + gLCl;
            this.EL = (gLK * this.EK + gLNa * this.ENa + gLCl * ELCl) / this.gL;
        }

        public override void SetIC(Double[] ic)
        {
            this.h = ic[3];
            this.m = ic[2];
            this.n = ic[1];
            this.V = ic[0];
        }

        public override Double[] ResetToFP()
        {
            this.h = 0.514071155409594D;// initial condition
            this.m = 0.069236269218388D;//
            this.n = 0.353721170200369D;
            this.V = -62.698490240702419D;
            return new Double[4] { this.V, this.n, this.m, this.h };
        }

        //public void TimeStep(Double Iext)
        public override void TimeStep()
        {
            this.initState[0] = this.h;
            this.initState[1] = this.m;
            this.initState[2] = this.n;
            this.initState[3] = this.V;
            this.c1 = HHStdModelStateEq(this.initState, 0.0D);
            this.currState[0] = this.initState[0] + this.dt * 0.5d * this.c1[0];
            this.currState[1] = this.initState[1] + this.dt * 0.5d * this.c1[1];
            this.currState[2] = this.initState[2] + this.dt * 0.5d * this.c1[2];
            this.currState[3] = this.initState[3] + this.dt * 0.5d * this.c1[3];
            this.c2 = HHStdModelStateEq(this.currState, 0.0D);
            this.currState[0] = this.initState[0] + this.dt * 0.5d * this.c2[0];
            this.currState[1] = this.initState[1] + this.dt * 0.5d * this.c2[1];
            this.currState[2] = this.initState[2] + this.dt * 0.5d * this.c2[2];
            this.currState[3] = this.initState[3] + this.dt * 0.5d * this.c2[3];
            this.c3 = HHStdModelStateEq(this.currState, 0.0D);
            this.currState[0] = this.initState[0] + this.dt * this.c3[0];
            this.currState[1] = this.initState[1] + this.dt * this.c3[1];
            this.currState[2] = this.initState[2] + this.dt * this.c3[2];
            this.currState[3] = this.initState[3] + this.dt * this.c3[3];
            this.c4 = HHStdModelStateEq(this.currState, 0.0D);
            this.h = this.initState[0] + (this.dt / 6.0d) * (this.c1[0] + 2.0d * this.c2[0] + 2.0d * this.c3[0] + this.c4[0]);
            this.m = this.initState[1] + (this.dt / 6.0d) * (this.c1[1] + 2.0d * this.c2[1] + 2.0d * this.c3[1] + this.c4[1]);
            this.n = this.initState[2] + (this.dt / 6.0d) * (this.c1[2] + 2.0d * this.c2[2] + 2.0d * this.c3[2] + this.c4[2]);
            this.V = this.initState[3] + (this.dt / 6.0d) * (this.c1[3] + 2.0d * this.c2[3] + 2.0d * this.c3[3] + this.c4[3]);
        }

        public override void TimeStepNet()
        {
            this.SumInputSignal();
            this.initState[0] = this.h;
            this.initState[1] = this.m;
            this.initState[2] = this.n;
            this.initState[3] = this.V;
            this.c1 = HHStdModelStateEq(this.initState, this.Isyn);
            this.currState[0] = this.initState[0] + this.dt * 0.5d * this.c1[0];
            this.currState[1] = this.initState[1] + this.dt * 0.5d * this.c1[1];
            this.currState[2] = this.initState[2] + this.dt * 0.5d * this.c1[2];
            this.currState[3] = this.initState[3] + this.dt * 0.5d * this.c1[3];
            this.c2 = HHStdModelStateEq(this.currState, this.Isyn);
            this.currState[0] = this.initState[0] + this.dt * 0.5d * this.c2[0];
            this.currState[1] = this.initState[1] + this.dt * 0.5d * this.c2[1];
            this.currState[2] = this.initState[2] + this.dt * 0.5d * this.c2[2];
            this.currState[3] = this.initState[3] + this.dt * 0.5d * this.c2[3];
            this.c3 = HHStdModelStateEq(this.currState, this.Isyn);
            this.currState[0] = this.initState[0] + this.dt * this.c3[0];
            this.currState[1] = this.initState[1] + this.dt * this.c3[1];
            this.currState[2] = this.initState[2] + this.dt * this.c3[2];
            this.currState[3] = this.initState[3] + this.dt * this.c3[3];
            this.c4 = HHStdModelStateEq(this.currState, this.Isyn);
            this.h = this.initState[0] + (this.dt / 6.0d) * (this.c1[0] + 2.0d * this.c2[0] + 2.0d * this.c3[0] + this.c4[0]);
            this.m = this.initState[1] + (this.dt / 6.0d) * (this.c1[1] + 2.0d * this.c2[1] + 2.0d * this.c3[1] + this.c4[1]);
            this.n = this.initState[2] + (this.dt / 6.0d) * (this.c1[2] + 2.0d * this.c2[2] + 2.0d * this.c3[2] + this.c4[2]);
            this.V = this.initState[3] + (this.dt / 6.0d) * (this.c1[3] + 2.0d * this.c2[3] + 2.0d * this.c3[3] + this.c4[3]);
        }

        private Double ff1(Double c, Double th, Double s, Double V)
        {
            return c * Math.Exp(s * (V - th));
        }

        private Double ff2(Double c, Double th, Double s, Double V)
        {
            return c / (1 + Math.Exp(s * (V - th)));
        }

        private Double ff3(Double c, Double th, Double s, Double V)
        {
            return c * (V - th) / (1 - Math.Exp(s * (V - th)));
        }

        private Double[] HHStdModelStateEq(Double[] y, Double Iext)
        {
            dydt[0] = ff1(0.07, -65.0, -0.05, y[3]) * (1 - y[0]) - ff2(1.0, -35.0, -0.1, y[3]) * y[0];//h
            dydt[1] = ff3(0.1, -40.0, -0.1, y[3]) * (1 - y[1]) - ff1(4.0, -65.0, -0.056, y[3]) * y[1];// m
            dydt[2] = ff3(0.01, -55.0, -0.1, y[3]) * (1 - y[2]) - ff1(0.125, -65.0, -0.013, y[3]) * y[2];// n equation
            dydt[3] = (this.Iext + Iext - this.gK * (y[2]*y[2]*y[2]*y[2]) * (y[3] - this.EK) - this.gNa * (y[1]*y[1]*y[1]) * y[0] * (y[3] - this.ENa) - this.gL * (y[3] - this.EL)) / this.C;//V
            return dydt;
        }

        public override Double GetV()
        {
            return this.V;
        }

        public override String ToString()
        {
            return "HHStdModel";
        }
    }

    public class HHLeechModel : Neuron
    {
        // HH parameters
        private Double gK2 = 30.0E-9; // conductance of the potassium channel
        private Double gl = 8.0E-9; // conductance of the leak current
        private Double gNa = 160.0E-9; // conductance of the sodium channel
        private Double EK = -0.07; // reversal potential of the K channel
        private Double ENa = 0.045; // reversal potential of the Na channel
        private Double El = -0.046; // leak reversal potencial
        private Double C = 0.5E-9; // the capacitance of the cell
        private Double VK2S = -0.0228; // parameter to shift the activation curve of IK2
        private Double tauK2 = 0.9; // time constant for K2 channel
        private Double tauNa = 0.0405; // time constant for Na channeln
        private Double A_HNA = 500.0;
        private Double B_HNA = 0.0325;
        private Double A_MK2 = -83.0;
        private Double B_MK2_LESS_VK2S = 0.018;
        private Double A_V = -150.0;
        private Double B_V = 0.0305;
        private Double Iext = 0.0062E-9;

        // HH variables
        private Double hNa = 0.3;//*_hNa; // pointer to the inactivation of INa
        private Double mK2 = 0.3;//*_mK2; // pointer to the activation of IK2
        private Double V = -0.05;//*_V; // pointer the potential of the cell
        private Double[] initState;
        private Double[] currState;
        private Double[] c1;
        private Double[] c2;
        private Double[] c3;
        private Double[] c4;
        private Double[] dydt;

        /*public Double dt { get; private set; } // the step of the integration
        public Double ts_per_ms { get; private set; }
        public Int32 transient { get; private set; }*/

        public HHLeechModel(NeuronRegime nr, Int32 totalTime)
        {
            this.Reset(nr, totalTime);
        }

        public override void Reset(NeuronRegime nr, Int32 totalTime)
        {
            this.initState = new Double[3];
            this.currState = new Double[3];
            this.c1 = new Double[3];
            this.c2 = new Double[3];
            this.c3 = new Double[3];
            this.c4 = new Double[3];
            this.dydt = new Double[3];
            this.dt = 0.01;
            this.ts_per_ms = 1 / this.dt;
            this.transient = 100; // in ms
            if (nr == NeuronRegime.Bursting)
            {
                this.SetBurstingParam();
            }
            else
            {
                this.SetExcitableParam();
            }
        }

        public override void SetBurstingParam()
        {
            this.gK2 = 30.0E-9; // conductance of the potassium channel
            this.gl = 8.0E-9; // conductance of the leak current
            this.gNa = 160.0E-9; // conductance of the sodium channel
            this.EK = -0.07; // reversal potential of the K channel
            this.ENa = 0.045; // reversal potential of the Na channel
            this.El = -0.046; // leak reversal potencial
            this.C = 0.5E-9; // the capacitance of the cell
            this.VK2S = -0.0228; // parameter to shift the activation curve of IK2
            this.tauK2 = 0.9; // time constant for K2 channel
            this.tauNa = 0.0405; // time constant for Na channeln
            this.A_HNA = 500.0;
            this.B_HNA = 0.0325;
            this.A_MK2 = -83.0;
            this.B_MK2_LESS_VK2S = 0.018;
            this.A_V = -150.0;
            this.B_V = 0.0305;
            this.Iext = 0.0062E-9;

            // HH variables
            this.hNa = 0.3;//*_hNa; // pointer to the inactivation of INa
            this.mK2 = 0.3;//*_mK2; // pointer to the activation of IK2
            this.V = -0.05;// -0.05;//*_V; // pointer the potential of the cell
        }

        public override void SetExcitableParam()
        {
            this.SetBurstingParam();
            this.VK2S = 0.0248;
        }

        public override void SetIC(Double[] ic)
        {
            this.hNa = ic[2];
            this.mK2 = ic[1];
            this.V = ic[0];
        }

        public override Double[] ResetToFP()
        {
            //Console.WriteLine("*** WARNING *** Unknown FP for the HHLeechModel");
            return new Double[3] { this.V, this.mK2, this.hNa };
        }

        //public void TimeStep(Double Iext)
        public override void TimeStep()
        {
            this.initState[0] = this.hNa;
            this.initState[1] = this.mK2;
            this.initState[2] = this.V;
            this.c1 = HHLeechModelStateEq(this.initState, 0.0D);
            this.currState[0] = this.initState[0] + this.dt * 0.5d * this.c1[0];
            this.currState[1] = this.initState[1] + this.dt * 0.5d * this.c1[1];
            this.currState[2] = this.initState[2] + this.dt * 0.5d * this.c1[2];
            this.c2 = HHLeechModelStateEq(this.currState, 0.0D);
            this.currState[0] = this.initState[0] + this.dt * 0.5d * this.c2[0];
            this.currState[1] = this.initState[1] + this.dt * 0.5d * this.c2[1];
            this.currState[2] = this.initState[2] + this.dt * 0.5d * this.c2[2];
            this.c3 = HHLeechModelStateEq(this.currState, 0.0D);
            this.currState[0] = this.initState[0] + this.dt * this.c3[0];
            this.currState[1] = this.initState[1] + this.dt * this.c3[1];
            this.currState[2] = this.initState[2] + this.dt * this.c3[2];
            this.c4 = HHLeechModelStateEq(this.currState, 0.0D);
            this.hNa = this.initState[0] + (this.dt / 6.0d) * (this.c1[0] + 2.0d * this.c2[0] + 2.0d * this.c3[0] + this.c4[0]);
            this.mK2 = this.initState[1] + (this.dt / 6.0d) * (this.c1[1] + 2.0d * this.c2[1] + 2.0d * this.c3[1] + this.c4[1]);
            this.V = this.initState[2] + (this.dt / 6.0d) * (this.c1[2] + 2.0d * this.c2[2] + 2.0d * this.c3[2] + this.c4[2]);
        }

        public override void TimeStepNet()
        {
            this.SumInputSignal();
            this.initState[0] = this.hNa;
            this.initState[1] = this.mK2;
            this.initState[2] = this.V;
            this.c1 = HHLeechModelStateEq(this.initState, this.Isyn);
            this.currState[0] = this.initState[0] + this.dt * 0.5d * this.c1[0];
            this.currState[1] = this.initState[1] + this.dt * 0.5d * this.c1[1];
            this.currState[2] = this.initState[2] + this.dt * 0.5d * this.c1[2];
            this.c2 = HHLeechModelStateEq(this.currState, this.Isyn);
            this.currState[0] = this.initState[0] + this.dt * 0.5d * this.c2[0];
            this.currState[1] = this.initState[1] + this.dt * 0.5d * this.c2[1];
            this.currState[2] = this.initState[2] + this.dt * 0.5d * this.c2[2];
            this.c3 = HHLeechModelStateEq(this.currState, this.Isyn);
            this.currState[0] = this.initState[0] + this.dt * this.c3[0];
            this.currState[1] = this.initState[1] + this.dt * this.c3[1];
            this.currState[2] = this.initState[2] + this.dt * this.c3[2];
            this.c4 = HHLeechModelStateEq(this.currState, this.Isyn);
            this.hNa = this.initState[0] + (this.dt / 6.0d) * (this.c1[0] + 2.0d * this.c2[0] + 2.0d * this.c3[0] + this.c4[0]);
            this.mK2 = this.initState[1] + (this.dt / 6.0d) * (this.c1[1] + 2.0d * this.c2[1] + 2.0d * this.c3[1] + this.c4[1]);
            this.V = this.initState[2] + (this.dt / 6.0d) * (this.c1[2] + 2.0d * this.c2[2] + 2.0d * this.c3[2] + this.c4[2]);
        }

        private Double[] HHLeechModelStateEq(Double[] y, Double Iext)
        {
            this.dydt = new Double[3];
            //this.dydt[0] = (1.0 / (1.0 + Math.Exp(this.A_HNA * (y[2] + this.B_HNA))) - y[0]) / this.tauNa;// hNa equation
            //this.dydt[1] = (1.0 / (1.0 + Math.Exp(this.A_MK2 * (y[2] + this.B_MK2_LESS_VK2S + this.VK2S))) - y[1]) / this.tauK2;// mK2 equation
            //this.dydt[2] = -(this.Iext + this.gK2 * y[1] * y[1] * (y[2] - EK) + this.gl * (y[2] - this.El) + this.gNa * y[0] * (y[2] - this.ENa) / ((1.0 + Math.Exp(this.A_V * (y[2] + this.B_V))) * (1.0 + Math.Exp(this.A_V * (y[2] + this.B_V))) * (1.0 + Math.Exp(this.A_V * (y[2] + this.B_V))))) / this.C;// V equation
            Double f3 = ff(this.A_V, this.B_V, y[2]);
            this.dydt[0] = (ff(this.A_HNA, this.B_HNA, y[2]) - y[0]) / this.tauNa; //h
            this.dydt[1] = (ff(this.A_MK2, this.B_MK2_LESS_VK2S + this.VK2S, y[2]) - y[1]) / this.tauK2;//m
            this.dydt[2] = -(this.Iext + Iext + this.gK2 * y[1] * y[1] * (y[2] - this.EK) + this.gNa * y[0] * (y[2] - this.ENa) * f3 * f3 * f3 + this.gl * (y[2] - this.El)) / this.C;//V
            return this.dydt;
        }

        private Double ff(Double a, Double b, Double V)
        {
            return 1.0D / (1.0D + Math.Exp(a * (V + b)));
        }

        public override Double GetV()
        {
            return this.V;
        }

        public override String ToString()
        {
            return "HHLeechModel";
        }
    }

    public abstract class Neuron : IModel
    {
        public Double ts_per_ms { get; protected set; }
        public Int32 transient { get; protected set; }
        public Double dt { get; protected set; }
        public Boolean isNetwork { get; private set; }
        public Int32 N { get; protected set; }

        protected System.Collections.Generic.List<GapJunction> Input { get; set; }

        protected Double Isyn { get; set; }

        protected Neuron()
        {
            this.Isyn = 0.0D;
            this.Input = new System.Collections.Generic.List<GapJunction>();
            this.isNetwork = false;
            this.N = 1;
        }

        protected virtual void SumInputSignal()
        {
            int i = 0, n = Input.Count;
            this.Isyn = 0.0D;
            while (i < n)
            {
                this.Isyn += this.Input[i].I;
                i++;
            }
        }

        public virtual void AddInput(GapJunction s)
        {
            this.Input.Add(s);
        }

        public abstract void SetIC(Double[] ic);
        public abstract Double[] ResetToFP();
        public abstract void TimeStepNet();
        //void TimeStep(Double Iext);
        public abstract void TimeStep();
        public abstract Double GetV();
        public virtual Double[] GetVNet()
        {
            return new Double[1];
        }
        public abstract void SetBurstingParam();
        public abstract void SetExcitableParam();
        public abstract void Reset(NeuronRegime nr, Int32 totalTime);
    }

    public interface IModel
    {
        Double ts_per_ms { get; }
        Int32 transient { get; }
        Double dt { get; }
        Boolean isNetwork { get; }
        Int32 N { get; }

        //void TimeStep(Double Iext);
        void TimeStep();
        Double GetV();
        Double[] GetVNet();
        void SetBurstingParam();
        void SetExcitableParam();
        void Reset(NeuronRegime nr, Int32 totalTime);
    }
}