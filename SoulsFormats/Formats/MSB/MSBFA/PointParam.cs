﻿using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSBFA
    {
        /// <summary>
        /// A collection of points and trigger volumes used by scripts and events.
        /// </summary>
        public class PointParam : Param<Region>, IMsbParam<IMsbRegion>
        {
            internal override int Version => throw new NotImplementedException();

            /// <summary>
            /// The name string indicating the param type this is.
            /// </summary>
            internal override string Name => "POINT_PARAM_ST";

            /// <summary>
            /// All regions in the map.
            /// </summary>
            public List<Region> Regions { get; set; }

            /// <summary>
            /// Creates an empty PointParam.
            /// </summary>
            public PointParam() : base()
            {
                Regions = new List<Region>();
            }

            /// <summary>
            /// Adds a region to the list; returns the region.
            /// </summary>
            public Region Add(Region region)
            {
                Regions.Add(region);
                return region;
            }
            IMsbRegion IMsbParam<IMsbRegion>.Add(IMsbRegion item) => Add((Region)item);

            /// <summary>
            /// Returns the list of regions.
            /// </summary>
            public override List<Region> GetEntries()
            {
                return Regions;
            }
            IReadOnlyList<IMsbRegion> IMsbParam<IMsbRegion>.GetEntries() => GetEntries();

            internal override Region ReadEntry(BinaryReaderEx br)
            {
                return Regions.EchoAdd(new Region(br));
            }
        }

        /// <summary>
        /// A point or volume used by scripts or events.
        /// </summary>
        public class Region : Entry, IMsbRegion
        {
            /// <summary>
            /// The name of the region.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Describes the physical shape of the region.
            /// </summary>
            public MSB.Shape Shape
            {
                get => _Shape;
                set
                {
                    if (value is MSB.Shape.Composite)
                        throw new ArgumentException("Armored Core: For Answer does not support composite shapes.");
                    _Shape = value;
                }
            }
            private MSB.Shape _Shape;

            /// <summary>
            /// Location of the region.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Rotation of the region, in degrees.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// Identifies the region in external files.
            /// </summary>
            public int EntityID { get; set; }

            /// <summary>
            /// Creates a Region with default values.
            /// </summary>
            public Region()
            {
                Name = "Region";
                Shape = new MSB.Shape.Point();
                EntityID = -1;
            }

            /// <summary>
            /// Creates a deep copy of the region.
            /// </summary>
            public Region DeepCopy()
            {
                var region = (Region)MemberwiseClone();
                region.Shape = Shape.DeepCopy();
                return region;
            }
            IMsbRegion IMsbRegion.DeepCopy() => DeepCopy();

            internal Region(BinaryReaderEx br)
            {
                throw new NotImplementedException();
            }

            internal override void Write(BinaryWriterEx bw, int id)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Returns a string representation of the region.
            /// </summary>
            public override string ToString()
            {
                return $"{Shape.Type} {Name}";
            }
        }
    }
}
