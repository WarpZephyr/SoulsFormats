﻿using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    public partial class SMD4
    {
        /// <summary>
        /// An individual chunk of a model.
        /// </summary>
        public class Mesh
        {
            /// <summary>
            /// The format of vertices in the vertex buffer.
            /// </summary>
            public byte VertexFormat { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unk01 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool Unk02 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool Unk03 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public short Unk06 { get; set; }

            /// <summary>
            /// Indexes of bones in the bone collection which may be used by vertices in this mesh.
            /// </summary>
            /// <remarks>
            /// Always has 28 indices; Unused indices are set to -1.
            /// </remarks>
            public short[] BoneIndices { get; set; }

            /// <summary>
            /// Get the number of used bone indices.
            /// </summary>
            public int BoneCount
            {
                get
                {
                    int count = 0;
                    for (int i = 0; i < 28; i++)
                    {
                        short index = BoneIndices[i];
                        if (index != -1)
                        {
                            count++;
                        }
                    }
                    return count;
                }
            }

            /// <summary>
            /// The vertex indices in this mesh.
            /// </summary>
            public List<ushort> Indices { get; set; }

            /// <summary>
            /// The vertices in this mesh.
            /// </summary>
            public List<Vertex> Vertices { get; set; }

            /// <summary>
            /// Create a new and empty Mesh with default values.
            /// </summary>
            public Mesh()
            {
                VertexFormat = 0;
                Unk01 = 0;
                Unk02 = true;
                Unk03 = false;
                Unk06 = 0;
                BoneIndices = new short[28];
                Indices = new List<ushort>();
                Vertices = new List<Vertex>();
                for (int i = 0; i < 28; i++)
                    BoneIndices[i] = -1;
            }

            /// <summary>
            /// Clone an existing Mesh.
            /// </summary>
            public Mesh(Mesh mesh)
            {
                VertexFormat = mesh.VertexFormat;
                Unk01 = mesh.Unk01;
                Unk02 = mesh.Unk02;
                Unk03 = mesh.Unk03;
                Unk06 = mesh.Unk06;
                BoneIndices = new short[28];
                Indices = new List<ushort>();
                Vertices = new List<Vertex>();
                for (int i = 0; i < 28; i++)
                    BoneIndices[i] = mesh.BoneIndices[i];
                for (int i = 0; i < mesh.Indices.Count; i++)
                    Indices[i] = mesh.Indices[i];
                for (int i = 0; i < mesh.Vertices.Count; i++)
                    Vertices[i] = new Vertex(mesh.Vertices[i]);
            }

            /// <summary>
            /// Read a new Mesh from a stream.
            /// </summary>
            internal Mesh(BinaryReaderEx br, int dataOffset, int version)
            {
                VertexFormat = br.ReadByte();
                Unk01 = br.ReadByte();
                Unk02 = br.ReadBoolean();
                Unk03 = br.ReadBoolean();
                ushort vertexIndexCount = br.ReadUInt16();
                Unk06 = br.ReadInt16();
                BoneIndices = br.ReadInt16s(28);
                int vertexIndicesLength = br.AssertInt32(vertexIndexCount * 2);
                int vertexIndicesOffset = br.ReadInt32();
                int vertexBufferLength = br.ReadInt32();
                int vertexBufferOffset = br.ReadInt32();

                Vertices = new List<Vertex>();
                Indices = new List<ushort>(br.GetUInt16s(dataOffset + vertexIndicesOffset, vertexIndexCount));

                br.StepIn(dataOffset + vertexBufferOffset);
                int vertexCount = vertexBufferLength / GetVertexSize(version);
                for (int i = 0; i < vertexCount; i++)
                {
                    Vertices.Add(new Vertex(br, version, VertexFormat));
                }
                br.StepOut();
            }

            /// <summary>
            /// Write this Mesh to a stream.
            /// </summary>
            internal void Write(BinaryWriterEx bw, int index, int version)
            {
                bw.WriteByte(VertexFormat);
                bw.WriteByte(Unk01);
                bw.WriteBoolean(Unk02);
                bw.WriteBoolean(Unk03);
                bw.WriteUInt16((ushort)Indices.Count);
                bw.WriteInt16(Unk06);
                bw.WriteInt16s(BoneIndices);
                bw.WriteInt32(Indices.Count * 2);
                bw.ReserveInt32($"vertexIndicesOffset_{index}");
                bw.WriteInt32(Vertices.Count * GetVertexSize(version));
                bw.ReserveInt32($"vertexBufferOffset_{index}");
            }

            /// <summary>
            /// Get the size of each Vertex.
            /// </summary>
            internal int GetVertexSize(int version)
            {
                if (version == 0x40001)
                {
                    if (VertexFormat == 0)
                    {
                        return 16;
                    }
                    else if (VertexFormat == 2)
                    {
                        return 36;
                    }
                    else
                    {
                        throw new NotSupportedException($"VertexFormat {VertexFormat} is not currently supported for Version {version}.");
                    }
                }
                else
                {
                    throw new NotSupportedException($"Version {version} is not currently supported.");
                }
            }

            /// <summary>
            /// Get a list of faces as index arrays.
            /// </summary>
            /// <param name="allowPrimitiveRestarts">Whether or not to allow primitive restarts.</param>
            /// <param name="includeDegenerateFaces">Whether or not to include degenerate faces.</param>
            /// <returns>A list of triangle arrays.</returns>
            public List<ushort[]> GetFaceIndices(bool allowPrimitiveRestarts, bool includeDegenerateFaces)
            {
                List<ushort> indices = Triangulate(allowPrimitiveRestarts, includeDegenerateFaces);
                var faces = new List<ushort[]>();
                for (int i = 0; i < indices.Count; i += 3)
                {
                    faces.Add(new ushort[]
                    {
                        indices[i + 0],
                        indices[i + 1],
                        indices[i + 2]
                    });
                }
                return faces;
            }

            /// <summary>
            /// Get an approximate triangle count for the mesh indices.
            /// </summary>
            /// <param name="allowPrimitiveRestarts">Whether or not to allow primitive restarts.</param>
            /// <param name="includeDegenerateFaces">Whether or not to include degenerate faces.</param>
            /// <returns>An approximate triangle count.</returns>
            public int GetFaceCount(bool allowPrimitiveRestarts, bool includeDegenerateFaces)
            {
                // Triangle strip
                int counter = 0;
                for (int i = 0; i < Indices.Count - 2; i++)
                {
                    int vi1 = Indices[i];
                    int vi2 = Indices[i + 1];
                    int vi3 = Indices[i + 2];

                    bool notRestart = allowPrimitiveRestarts || (vi1 != 0xFFFF && vi2 != 0xFFFF && vi3 != 0xFFFF);
                    bool included = includeDegenerateFaces || (vi1 != vi2 && vi1 != vi3 && vi2 != vi3);
                    if (notRestart && included)
                    {
                        counter++;
                    }
                }

                return counter;
            }

            /// <summary>
            /// Attempt to triangulate the mesh face indices.
            /// </summary>
            /// <param name="allowPrimitiveRestarts">Whether or not to allow primitive restarts.</param>
            /// <param name="includeDegenerateFaces">Whether or not to include degenerate faces.</param>
            /// <returns>A triangulated list of face indices.</returns>
            public List<ushort> Triangulate(bool allowPrimitiveRestarts, bool includeDegenerateFaces)
            {
                var triangles = new List<ushort>();
                bool flip = false;
                for (int i = 0; i < Indices.Count - 2; i++)
                {
                    ushort vi1 = Indices[i];
                    ushort vi2 = Indices[i + 1];
                    ushort vi3 = Indices[i + 2];

                    if (allowPrimitiveRestarts && (vi1 == 0xFFFF || vi2 == 0xFFFF || vi3 == 0xFFFF))
                    {
                        flip = true;
                    }
                    else
                    {
                        if (includeDegenerateFaces || (vi1 != vi2 && vi2 != vi3 && vi3 != vi1))
                        {
                            if (flip)
                            {
                                triangles.Add(vi3);
                                triangles.Add(vi2);
                                triangles.Add(vi1);
                            }
                            else
                            {
                                triangles.Add(vi1);
                                triangles.Add(vi2);
                                triangles.Add(vi3);
                            }
                        }
                        flip = !flip;
                    }
                }

                return triangles;
            }
        }
    }
}
