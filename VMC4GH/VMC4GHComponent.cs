using System;
using System.Collections.Generic;
using System.Linq;
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
            pManager.AddPointParameter("Hips", "Hips", "Hips", GH_ParamAccess.item);
            pManager.AddPointParameter("LeftUpperLeg", "LeftUpperLeg", "LeftUpperLeg", GH_ParamAccess.item);
            pManager.AddPointParameter("LeftLowerLeg", "LeftLowerLeg", "LeftLowerLeg", GH_ParamAccess.item);
            pManager.AddPointParameter("LeftFoot", "LeftFoot", "LeftFoot", GH_ParamAccess.item);
            pManager.AddPointParameter("LeftToes", "LeftToes", "LeftToes", GH_ParamAccess.item);
            pManager.AddPointParameter("RightUpperLeg", "RightUpperLeg", "RightUpperLeg", GH_ParamAccess.item);
            pManager.AddPointParameter("RightLowerLeg", "RightLowerLeg", "RightLowerLeg", GH_ParamAccess.item);
            pManager.AddPointParameter("RightFoot", "RightFoot", "RightFoot", GH_ParamAccess.item);
            pManager.AddPointParameter("RightToes", "RightToes", "RightToes", GH_ParamAccess.item);
            pManager.AddPointParameter("Spine", "Spine", "Spine", GH_ParamAccess.item);
            pManager.AddPointParameter("Chest", "Chest", "Chest", GH_ParamAccess.item);
            pManager.AddPointParameter("UpperChest", "UpperChest", "UpperChest", GH_ParamAccess.item);
            pManager.AddPointParameter("LeftShoulder", "LeftShoulder", "LeftShoulder", GH_ParamAccess.item);
            pManager.AddPointParameter("RightShoulder", "RightShoulder", "RightShoulder", GH_ParamAccess.item);
            pManager.AddPointParameter("LeftUpperArm", "LeftUpperArm", "LeftUpperArm", GH_ParamAccess.item);
            pManager.AddPointParameter("LeftLowerArm", "LeftLowerArm", "LeftLowerArm", GH_ParamAccess.item);
            pManager.AddPointParameter("LeftHand", "LeftHand", "LeftHand", GH_ParamAccess.item);
            pManager.AddPointParameter("RightUpperArm", "RightUpperArm", "RightUpperArm", GH_ParamAccess.item);
            pManager.AddPointParameter("RightLowerArm", "RightLowerArm", "RightLowerArm", GH_ParamAccess.item);
            pManager.AddPointParameter("RightHand", "RightHand", "RightHand", GH_ParamAccess.item);
            pManager.AddPointParameter("Neck", "Neck", "Neck", GH_ParamAccess.item);
            pManager.AddPointParameter("Head", "Head", "Head", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var port = 0;
            if (_bonePosition == null)
            {
                _bonePosition = BoneInitialize();
            }
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
                    switch (message.Address)
                    {
                        case "/VMC/Ext/Bone/Pos":
                            _bonePosition[message[0].ToString()] = new Point3d(
                                double.Parse(message[1].ToString()),
                                double.Parse(message[3].ToString()),
                                double.Parse(message[2].ToString())
                                );
                            break;
                    }
                }
            }

            Dictionary<string, Point3d> pts = GetBonePos();
            foreach (KeyValuePair<string, Point3d> pair in pts.Where(pair => BoneInitialize().ContainsKey(pair.Key)))
            {
                DA.SetData(pair.Key, pair.Value);
            }
        }

        private Dictionary<string, Point3d> BoneInitialize()
        {
            return new Dictionary<string, Point3d>
            {
                {"Hips", Point3d.Origin},
                {"LeftUpperLeg", Point3d.Origin},
                {"LeftLowerLeg", Point3d.Origin},
                {"LeftFoot", Point3d.Origin},
                {"LeftToes", Point3d.Origin},
                {"RightUpperLeg", Point3d.Origin},
                {"RightLowerLeg", Point3d.Origin},
                {"RightFoot", Point3d.Origin},
                {"RightToes", Point3d.Origin},
                {"Spine", Point3d.Origin},
                {"Chest", Point3d.Origin},
                {"UpperChest", Point3d.Origin},
                {"LeftShoulder", Point3d.Origin},
                {"RightShoulder", Point3d.Origin},
                {"LeftUpperArm", Point3d.Origin},
                {"LeftLowerArm", Point3d.Origin},
                {"LeftHand", Point3d.Origin},
                {"RightUpperArm", Point3d.Origin},
                {"RightLowerArm", Point3d.Origin},
                {"RightHand", Point3d.Origin},
                {"Neck", Point3d.Origin},
                {"Head", Point3d.Origin},
            };
        }

        private Dictionary<string, Point3d> GetBonePos()
        {
            //TODO: Last との差分で計算するようにする
            var ptsDict = new Dictionary<string, Point3d>();
            foreach (KeyValuePair<string, Point3d> pair in _bonePosition)
            {
                switch (pair.Key)
                {
                    case "Hips":
                        ptsDict[pair.Key] = pair.Value;
                        break;
                    case "LeftUpperLeg":
                    case "RightUpperLeg":
                    case "Spine":
                        ptsDict[pair.Key] = pair.Value + _bonePosition["Hips"];
                        break;
                    case "LeftLowerLeg":
                        ptsDict[pair.Key] = pair.Value + _bonePosition["Hips"] + _bonePosition["LeftUpperLeg"];
                        break;
                    case "LeftFoot":
                        ptsDict[pair.Key] = pair.Value + _bonePosition["Hips"] + _bonePosition["LeftUpperLeg"] + _bonePosition["LeftLowerLeg"];
                        break;
                    case "LeftToes":
                        ptsDict[pair.Key] = pair.Value + _bonePosition["Hips"] + _bonePosition["LeftUpperLeg"] + _bonePosition["LeftLowerLeg"] + _bonePosition["LeftFoot"];
                        break;
                    case "RightLowerLeg":
                        ptsDict[pair.Key] = pair.Value + _bonePosition["Hips"] + _bonePosition["RightUpperLeg"];
                        break;
                    case "RightFoot":
                        ptsDict[pair.Key] = pair.Value + _bonePosition["Hips"] + _bonePosition["RightUpperLeg"] + _bonePosition["RightLowerLeg"];
                        break;
                    case "RightToes":
                        ptsDict[pair.Key] = pair.Value + _bonePosition["Hips"] + _bonePosition["RightUpperLeg"] + _bonePosition["RightLowerLeg"] + _bonePosition["RightFoot"];
                        break;
                    case "Chest":
                        ptsDict[pair.Key] = pair.Value + _bonePosition["Hips"] + _bonePosition["Spine"];
                        break;
                    case "UpperChest":
                        ptsDict[pair.Key] = pair.Value + _bonePosition["Hips"] + _bonePosition["Spine"] + _bonePosition["Chest"];
                        break;
                    case "LeftShoulder":
                    case "RightShoulder":
                    case "Neck":
                        ptsDict[pair.Key] = pair.Value + _bonePosition["Hips"] + _bonePosition["Spine"] + _bonePosition["Chest"] + _bonePosition["UpperChest"];
                        break;
                    case "LeftUpperArm":
                        ptsDict[pair.Key] = pair.Value + _bonePosition["Hips"] + _bonePosition["Spine"] + _bonePosition["Chest"] + _bonePosition["UpperChest"]
                                            + _bonePosition["LeftShoulder"];
                        break;
                    case "LeftLowerArm":
                        ptsDict[pair.Key] = pair.Value + _bonePosition["Hips"] + _bonePosition["Spine"] + _bonePosition["Chest"] + _bonePosition["UpperChest"]
                                            + _bonePosition["LeftShoulder"] + _bonePosition["LeftUpperArm"];
                        break;
                    case "LeftHand":
                        ptsDict[pair.Key] = pair.Value + _bonePosition["Hips"] + _bonePosition["Spine"] + _bonePosition["Chest"] + _bonePosition["UpperChest"]
                                            + _bonePosition["LeftShoulder"] + _bonePosition["LeftUpperArm"] + _bonePosition["LeftLowerArm"];
                        break;
                    case "RightUpperArm":
                        ptsDict[pair.Key] = pair.Value + _bonePosition["Hips"] + _bonePosition["Spine"] + _bonePosition["Chest"] + _bonePosition["UpperChest"]
                                            + _bonePosition["RightShoulder"];
                        break;
                    case "RightLowerArm":
                        ptsDict[pair.Key] = pair.Value + _bonePosition["Hips"] + _bonePosition["Spine"] + _bonePosition["Chest"] + _bonePosition["UpperChest"]
                                            + _bonePosition["RightShoulder"] + _bonePosition["RightUpperArm"];
                        break;
                    case "RightHand":
                        ptsDict[pair.Key] = pair.Value + _bonePosition["Hips"] + _bonePosition["Spine"] + _bonePosition["Chest"] + _bonePosition["UpperChest"]
                                            + _bonePosition["RightShoulder"] + _bonePosition["RightUpperArm"] + _bonePosition["RightLowerArm"];
                        break;
                    case "Head":
                        ptsDict[pair.Key] = pair.Value + _bonePosition["Hips"] + _bonePosition["Spine"] + _bonePosition["Chest"] + _bonePosition["UpperChest"] + _bonePosition["Neck"];
                        break;
                    default:
                        ptsDict[pair.Key] = Point3d.Origin;
                        break;
                }
            }

            return ptsDict;
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b81281cd-17e2-41f6-b5e5-d877abdfe873");
    }
}
