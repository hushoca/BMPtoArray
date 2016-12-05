using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BitmapToArray
{
    public partial class Form1 : Form
    {
        //init the path variable which will be used to keep BMP location
        String path = null;

        public Form1()
        {
            InitializeComponent();
            //init the filter so only bmp files can be chosen
            FileDialog.Filter = "Bitmap File (*.bmp)|*.bmp";
        }

        /// <summary>
        /// Convert RGB values to 16-bit hexadecimal color.
        /// </summary>
        /// <param name="r">8-bit red value (0-255)</param>
        /// <param name="g">8-bit green value (0-255)</param>
        /// <param name="b">8-bit blue value (0-255)</param>
        /// <returns>16Bit RGB color</returns>
        public String to16BitColor(UInt32 r, UInt32 g, UInt32 b)
        {
            //convert all the colors to new sizes
            UInt16 red      = (UInt16)( (31 * r) / 255 ); //convert red to 5 bits
            UInt16 green    = (UInt16)( (63 * g) / 255 ); //convert red to to 6 bits
            UInt16 blue     = (UInt16)( (31 * b) / 255 ); //convert red to to 5 bits

            //init the variable which will keep the result
            UInt16 result = 0;

            //shift red 11 bits to left so it gets in position
            //original = 0000 0000 0001 1111
            //after shifting = 1111 1000 0000 0000
            red = (UInt16)(red << 11);

            //shift green 5 bits to left so its set into correct position
            //original = 0000 0000 0011 1111
            //after shifting = 0000 0111 1110 0000
            green = (UInt16)(green << 5);

            //no change is made to blue since its supposed to be at first 5 bits

            //lets say  r = 1111 1000 0000 0000
            //          g = 0000 0000 1110 0000
            //          b = 0000 0000 0000 1010
            //---------------------------------
            //after OR op = 1111 1000 1110 1010

            //use OR to put all the bits together
            result = (UInt16)( red | green | blue);

            //put 0x to start and make ABCDEF capital for visuals...
            String hexS = "0x" + Convert.ToString(result, 16).ToUpper();

            //return the value
            return hexS;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //open the select BMP dialog
            FileDialog.ShowDialog();
        }

        /// <summary>
        /// Gets only called when a file is selected successfully
        /// </summary>
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            //set path variable to filename
            path = FileDialog.FileName;
            textBox1.Text = path;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //check to make sure a file is selected
            if (path != null) {
                //open the BMP file from path
                Bitmap bmp = new Bitmap(path);

                //graphics unit pixel for later use
                GraphicsUnit px = GraphicsUnit.Pixel;

                //get height and width (unit = pixels using px variable)
                int height = (int) bmp.GetBounds(ref px).Height;
                int width = (int) bmp.GetBounds(ref px).Width;

                //check to make sure size is 128*128 
                //(**remove this if check to make the application universal**)
                if (height == 128 && width == 128.0) {

                    //create a new StringBuilder to use as string buffer
                    StringBuilder buffer = new StringBuilder();

                    //add buffer the init array
                    int size = height * width;
                    buffer.Append("uint16_t img[" + size.ToString() + "] = {");

                    //up to down cycle
                    for (int h = 0; h < height; h++)
                    {

                        //left to right cycle
                        for (int w = 0; w < width; w++) {
                            //here the code will go through each pixel in the line
                            //and jump to another line when that line is finished.

                            //get the pixel we are at with x = w and y = h.
                            Color c = bmp.GetPixel(w, h);

                            //get the RGB values of the pixel
                            UInt32 red = c.R;
                            UInt32 green = c.G;
                            UInt32 blue = c.B;

                            //conver the rgb values to 16bix hex and add it to buffer with
                            //a "," followed by the value
                            buffer.AppendFormat("{0},", to16BitColor(red,green,blue));                          
                        }
                        //jump to another line when we are at a new height (for visual effect)
                        //if this is removed array would be 1 line.
                        buffer.Append("\n");
                    }

                    //remove the last 2 item on the buffer (which is an unnecessary "," and "\n")
                    buffer.Remove(buffer.Length-2,2);

                    //close the array
                    buffer.Append("}");

                    //put the array text into the richtextbox
                    richTextBox1.Text = buffer.ToString();

                    //copy the array text to clipboard and inform the user
                    Clipboard.SetText(buffer.ToString());
                    MessageBox.Show("Data has been copied to your clipboard!","Information",MessageBoxButtons.OK,MessageBoxIcon.Information);

                } else //IF SIZE IS NOT EQUAL TO 128*128
                {
                    //warn the user!
                    MessageBox.Show("Bitmap must be 128*128 pixels! (Current size is "+
                        width.ToString()+"*"+height.ToString()+
                        ")", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
            }
        }
    }
}
