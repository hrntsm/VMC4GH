using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace VMC4GH
{
    public class VMC4GHInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "VMC4GH";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("5b5f375e-9377-4375-8c76-51c83ae38454");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
