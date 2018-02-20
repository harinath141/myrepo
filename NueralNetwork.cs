using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NueralNetworks
{
    public class NueralNetwork
    {
        #region datavariables
        private int InputSize { get; set; }
        private int HiddenLayer { get; set; }//excluding inputlayer & outputlayer
        private int LayerSize { get; set; } //size of hidden layers
        private int OutputSize { get; set; } //no of classes to recognize
        private double[] weights { get; set; } //weights
        private double[] bias { get; set; } //bias
        #endregion
        //dummy constructor
        public NueralNetwork()
        {
            this.InputSize = 3;
            this.HiddenLayer = 2;
            this.LayerSize = 3;
            this.OutputSize = 1;
            this.weights = new double[InputSize];
            this.bias = new double[LayerSize];
           var rnd = new Random();
            for (int i = 0; i < InputSize; i++)
            {                
                double item = rnd.NextDouble();
                weights[i]= item;
            }
            for (int i = 0; i < LayerSize; i++)
            {
                double item = rnd.NextDouble();
                bias[i] = item;
            }
        }

        public void summingOperation()
        {

        }
    }
    static class TransferFunctions
    {
        public static double Evaluate(TransferFunction transferFunction, double input)
        {
            switch (transferFunction)
            {
                case TransferFunction.Sigmoid:
                    return Sigmoid(input);
                case TransferFunction.Linear:
                    return Linear(input);
                case TransferFunction.Gaussian:
                    return Gaussian(input);
                case TransferFunction.RationalSigmoid:
                    return RationalSigmoid(input);
                default:
                    return 0.0;
            }
        }

        public static double EvaluateDerivative(TransferFunction transferFunction, double input)
        {
            switch (transferFunction)
            {
                case TransferFunction.Sigmoid:
                    return SigmoidDerivative(input);
                case TransferFunction.Linear:
                    return LinearDerivative(input);
                case TransferFunction.Gaussian:
                    return GaussianDerivative(input);
                case TransferFunction.RationalSigmoid:
                    return RationalSigmoidDerivative(input);
                default:
                    return 0.0;
            }
        }

        //actual transfer functions
        private static double Sigmoid(double x)
        {
            var result = 1.0 / (1.0 + Math.Exp(-x));

            return result;
        }

        private static double SigmoidDerivative(double x)
        {
            var result = Sigmoid(x) * (1 - Sigmoid(x));

            return result;
        }

        private static double Linear(double x)
        {
            return x;
        }

        private static double LinearDerivative(double x)
        {
            return 1;
        }

        private static double Gaussian(double x)
        {
            return Math.Exp(-Math.Pow(x, 2));
        }

        private static double GaussianDerivative(double x)
        {
            return (-2 * x * Gaussian(x));
        }

        private static double RationalSigmoid(double x)
        {
            return (x / (1.0 + Math.Sqrt(1.0 + x * x)));
        }

        private static double RationalSigmoidDerivative(double x)
        {
            double val = Math.Sqrt(1 + x * x);

            return (1.0 / val * (1 + val));
        }
    }
    public enum TransferFunction
    {
        None,
        Sigmoid,
        Linear,
        Gaussian,
        RationalSigmoid
    }
}