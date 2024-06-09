/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

namespace NRKernal
{
    /// <summary> Request flags for the meshing system. </summary>
    public enum NRMeshingFlags
    {
        NR_MESHING_FLAGS_COMPUTE_NORMAL = 1 << 0,
        NR_MESHING_FLAGS_NULL = 0,
    };

    /// <summary> State of a block mesh. </summary>
    public enum NRMeshingBlockState
    {
        /// Block mesh has been created.
        NR_MESHING_BLOCK_STATE_NEW,
        /// Block mesh has been updated.
        NR_MESHING_BLOCK_STATE_UPDATED,
        /// Block mesh has been deleted.
        NR_MESHING_BLOCK_STATE_DELETED,
        /// Block mesh is unchanged.
        NR_MESHING_BLOCK_STATE_UNCHANGED,
    };

    /// <summary> Struct contains information of a block. </summary>
    public struct BlockInfo
    {
        public ulong identifier;
        public ulong timestamp;
        public NRMeshingBlockState blockState;
        public NRMeshingFlags meshingFlag;
    }
}
