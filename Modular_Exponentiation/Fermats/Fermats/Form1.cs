using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fermats
{
    public partial class Form1 : Form
    {
        ErrorProvider errorP = new ErrorProvider();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //If we have good input
            if ((input.Text != "") && (kValue.Text != ""))
            {
                //See if input is correct (integers)
                int inputValue;
                int inputKValue;
                if ((int.TryParse(input.Text, out inputValue)) && (int.TryParse(kValue.Text, out inputKValue))) // Input is numeric
                {
                    //Get the values for P and K
                    Int32 p = Convert.ToInt32(input.Text);
                    Int32 k = Convert.ToInt32(kValue.Text);

                    if ((p >= 2) && (k >= 1)) //Numeric input is correct
                    {
                        errorP.Clear();
                        //Now test the primality of the given number P, using K number of tests.
                        if (primality(p, k))
                        {
                            //We know that the number is probably prime with a probability of 1/(2^k)
                            double probability = (1 - (1 /(Math.Pow(2,k)))) * 100;
                            output.Text = "Yes, with a probability of " + probability.ToString() + " %";
                        }
                        else
                        {
                            //We know the number is not prime.
                            output.Text = "No";
                        }
                    }
                    else
                    { errorOccured("oops"); }
                }
                else
                { errorOccured(); }      
            }
            else
            { errorOccured(); }

        }

        private bool primality(Int32 p, Int32 k) //This algorithm runs at Big O(k * n^3) Time
        {
            
            bool isFalseAtLeastOnce = false;
            Random random = new Random();

            //For K number of tests
            for (int i = 0; i < k; i++) // Big O(k) Time ----
            {
               //Generate a random integer that is in the range of {1 .... p-1}
               //Assign that number as a.
               Int32 a = random.Next(1, p); // it will bound it to p as top, but the highest number it will have is p-1

               //If the modular exponentiation doesn't render a 1, then the number fails and is definitely NOT prime

               if(modexp(a, p - 1, p) != 1) //Big O(n^3) Time ----
               {
                    //Make note that the number fails
                    isFalseAtLeastOnce = true;
                    //We don't need to test anymore
                    break;
               }   
            }

            //The number fails and is not PRIME
            if (isFalseAtLeastOnce)
            {
                return false;
            }
            else // The number didn't fail any of the tests and is probably PRIME (we will give a probability)
            {
                return true;
            }
        }


        private Int32 modexp(Int32 x, Int32 y, Int32 N) //This algorithm runs at a Big O(n^3) Time
        {
            //Base case
            if (y == 0)
            {
                return 1;
            }

            Int32 z = modexp(x, (y / 2), N);
            if ((y % 2) == 0) // We know we have an even number 
            {
                return (z * z) % N;
            }
            else // we have an odd number
            {
                return (x * (z * z)) % N;
            }

        }

        private void errorOccured()
        {
            //Please provide correct information
            errorP.SetError(input, "Please enter an integer to test");
            errorP.SetError(kValue, "Please enter an integer for K value");
            //Clear the output text
            output.Text = "";
        }
         
        private void errorOccured(string anything)
        {
            //Please provide correct information
            errorP.SetError(input, "The number to test must be at least 2");
            errorP.SetError(kValue, "The number of tests must be at least 1");
            //Clear the output text
            output.Text = "";
        }


    }
}
