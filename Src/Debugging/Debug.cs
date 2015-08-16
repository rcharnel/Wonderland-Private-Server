using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Wonderland_Private_Server.Src.Debugging
{
    public static class Debug
    {
        static RichTextBox rtfbox;
        static TextBox txtbox;

        public static void Initialize(ref TextBox src)
        {
            txtbox = src;
        }
        public static void Initialize(ref RichTextBox src)
        {
            rtfbox = src;
        }


    }
}
