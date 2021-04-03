using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rug.Osc;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace VMC4GH
{
    public class VMC4GHComponent : GH_Component
    {
        private static OscReceiver s_receiver;
        private Dictionary<string, Point3d> _bonePosition;

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public VMC4GHComponent()
          : base("VMC4GH", "Nickname",
              "Description",
              "Category", "Subcategory")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("port", "port", "port", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("out", "out", "out", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var port = 0;
            var result = new List<string>();
            if (!DA.GetData(0, ref port)) { return; };

            if (s_receiver == null || s_receiver.State != OscSocketState.Connected)
            {
                s_receiver = new OscReceiver(port);
                s_receiver.Connect();
            }

            if (s_receiver.State == OscSocketState.Connected && s_receiver.Receive() is OscBundle bundle)
            {
                foreach (OscMessage message in bundle.Where(b => b is OscMessage))
                {
                    var item = message;
                    switch (message.Address)
                    {
                        case "/VMC/Ext/Bone/Pos":
                            _bonePosition[message[0].ToString()] = new Point3d(
                                double.Parse(message[1].ToString()),
                                double.Parse(message[2].ToString()),
                                double.Parse(message[3].ToString())
                                );
                            break;
                    }
                }
            }

            DA.SetDataList(0, _bonePosition);
        }

        private static Dictionary<string, Point3d> InitBonePosition()
        {
            return new Dictionary<string, Point3d> { { "Hips", Point3d.Origin } };
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b81281cd-17e2-41f6-b5e5-d877abdfe873"); }
        }
    }
}
