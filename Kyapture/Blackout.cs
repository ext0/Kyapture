using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kyapture
{
    public partial class Blackout : Form
    {
        public Blackout() : base()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            AllowTransparency = true;
            DoubleBuffered = true;
        }
    }
}
