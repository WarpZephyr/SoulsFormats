﻿namespace SoulsFormats
{
    public partial class AcParts4
    {
        /// <summary>
        /// A Stabilizer in the middle of Leg parts in an ACPARTS file.
        /// </summary>
        public class LegMiddleStabilizer : IPart, IStabilizer
        {
            /// <summary>
            /// A Component which contains common stats across all parts.
            /// </summary>
            public PartComponent PartComponent { get; set; }

            /// <summary>
            /// A Component which contains Stabilizer stats.
            /// </summary>
            public StabilizerComponent StabilizerComponent { get; set; }

            /// <summary>
            /// Creates a new Leg Middle Stabilizer part.
            /// </summary>
            /// <param name="br">A binary reader.</param>
            /// <param name="version">The version indicating which 4thgen game's AcParts is being read.</param>
            internal LegMiddleStabilizer(BinaryReaderEx br, AcParts4Version version)
            {
                PartComponent = new PartComponent(br, version);
                StabilizerComponent = new StabilizerComponent(br);
            }

            /// <summary>
            /// Writes a Leg Middle Stabilizer part to a stream.
            /// </summary>
            /// <param name="bw">A binary writer.</param>
            /// <param name="version">The version indicating which 4thgen game's AcParts is being written.</param>
            public void Write(BinaryWriterEx bw, AcParts4Version version)
            {
                PartComponent.Write(bw, version);
                StabilizerComponent.Write(bw);
            }
        }
    }
}