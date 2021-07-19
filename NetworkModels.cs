using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeuronPerformance.Models
{
    public enum NetworkType
    {
        // line of N neurons
        Linear,
        // full graph network
        MeanField
    }
    public class NetworkModel : IModel
    {
        private NetworkType netType { get; set; }
        private NeuronType neuType { get; set; }
        private NeuronRegime neuReg { get; set; }
        public Double ts_per_ms { get; private set; }
        public Int32 transient { get; private set; }
        public Int32 totalTime { get; private set; }
        public Double dt { get; private set; }
        public Boolean isNetwork { get; private set; }

        private List<GapJunction> synapses { get; set; }
        private List<Neuron> neurons { get; set; }
        private Int32 nSyn { get; set; }
        public Double synG { get; private set; }
        public Int32 N { get; private set; }

        private Action<NeuronRegime, Int32> CreateNetwork { get; set; }

        public NetworkModel(NeuronType neut, NeuronRegime nr, Int32 totalTime, NetworkType nt, Int32 N, Double synG)
        {
            this.isNetwork = true;
            this.netType = nt;
            this.neuType = neut;
            this.neuReg = nr;
            this.totalTime = totalTime;
            this.N = N;
            this.synG = synG;
            if (nt == NetworkType.Linear)
                this.CreateNetwork = this.CreateNetworkLinear;
            else if (nt == NetworkType.MeanField)
                this.CreateNetwork = this.CreateNetworkMeanField;
            else
                throw new ArgumentOutOfRangeException("Unrecognized NetworkType");
            this.Initialize();
        }

        private void Initialize()
        {
            this.CreateNeurons(this.neuType, this.neuReg, this.totalTime);
            this.CreateNetwork(this.neuReg, this.totalTime);
        }

        public void CreateNetworkLinear(NeuronRegime nr, Int32 totalTime)
        {
            this.synapses = new List<GapJunction>();
            Int32 i = 1;
            while (i < this.N)
            {
                this.synapses.Add(new GapJunction(this.neurons[i-1], this.neurons[i], this.synG));
                this.neurons[i].AddInput(this.synapses.Last());
                i++;
            }
            this.nSyn = this.synapses.Count;
            this.neurons.ForEach(n => n.ResetToFP()); // only neuron 0 has activity in t = 0
            this.neurons[0].Reset(nr, totalTime);
        }

        public void CreateNetworkMeanField(NeuronRegime nr, Int32 totalTime)
        {
            this.synapses = new List<GapJunction>();
            Int32 j, i = 0;
            while (i < this.N)
            {
                j = 0;
                while (j < this.N)
                {
                    if (i != j)
                    {
                        this.synapses.Add(new GapJunction(this.neurons[i], this.neurons[j], this.synG));
                        this.neurons[j].AddInput(this.synapses.Last());
                    }
                    j++;
                }
                i++;
            }
            this.nSyn = this.synapses.Count;
            Double[] ic = this.neurons[0].ResetToFP();
            Random r = new Random();
            this.neurons.ForEach(n => n.SetIC(ic.Select(x => x * r.NextDouble()).ToArray())); // randomizing IC for all neurons
        }

        private void CreateNeurons(NeuronType neut, NeuronRegime nr, Int32 totalTime)
        {
            this.neurons = new List<Neuron>(this.N);
            Int32 i = 0;
            while (i < this.N)
            {
                this.neurons.Add(NeuronFactory.Create(neut, nr, totalTime));
                i++;
            }
            this.ts_per_ms = this.neurons[0].ts_per_ms;
            this.transient = this.neurons[0].transient;
            this.dt = this.neurons[0].dt;
        }

        //void TimeStep(Double Iext);
        public void TimeStep()
        {
            // network timestep
            Int32 j = 0;
            while (j < this.nSyn)
            {
                this.synapses[j].TimeStep();
                j++;
            }
            j = 0;
            while (j < this.N)
            {
                this.neurons[j].TimeStepNet();
                j++;
            }
        }

        public Double GetV()
        {
            // gets the absolute difference between membrane potential of all neurons
            // or the sum of all V??
            return this.neurons.Sum(n => n.GetV());
        }
        public Double[] GetVNet()
        {
            return this.neurons.Select(n => n.GetV()).ToArray();
        }
        public void SetBurstingParam()
        {
            // sets network neurons with bursting param
            this.neurons.ForEach(n => n.SetBurstingParam());
        }
        public void SetExcitableParam()
        {
            // sets network neurons with excitable params
            this.neurons.ForEach(n => n.SetExcitableParam());
        }
        public void Reset(NeuronRegime nr, Int32 totalTime)
        {
            // resets network
            //this.synapses.ForEach(s => s.Reset());
            //this.neurons.ForEach(n => n.Reset(nr, totalTime));
            this.neuReg = nr;
            this.totalTime = totalTime;
            this.Initialize();
        }
        public override String ToString()
        {
            return this.netType.ToString() + "_" + this.neuType.ToString() + this.neuReg.ToString();
        }
    }

    public class GapJunction
    {
        public Neuron preSyn { get; private set; }

        public Neuron postSyn { get; private set; }

        /// <summary>
        /// synaptic conductance (coupling intensity)
        /// </summary>
        public Double G { get; private set; }

        public GapJunction(Neuron nPre, Neuron nPost, Double G)
        {
            this.preSyn = nPre;
            this.postSyn = nPost;
            this.G = G;
            this.Reset();
        }

        /// <summary>
        /// synaptic current for a given time step
        /// </summary>
        public Double I { get; private set; }
        
        public void TimeStep()
        {
            this.I = this.G * (this.preSyn.GetV() - this.postSyn.GetV());
        }

        public void Reset()
        {
            this.I = 0.0d;
        }
    }
}