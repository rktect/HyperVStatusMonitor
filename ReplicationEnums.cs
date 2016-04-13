using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HyperVStatusMon
{
    /// <summary>
    /// ConvertTo-Json converts enums to their int values (can't override this to my knowledge).
    /// For some reason, our server shows the value of 4 but enum label of "Replicating", which should be value 3 according to documentation I could find.
    /// So this enum is hacked for now and hopefully the other values are correct.
    /// </summary>
    public enum ReplicationState
    {
        NA = 0,
        ReadyForInitialReplication = 1,
        WaitingForInitialReplication = 2,
        Unknown = 3,
        Replicating = 4,
        SyncedReplicationComplete = 4,
        FailOverWaitingCompletion = 5,
        FailedOver = 6,
        Suspended = 7,
        Error = 8,
        WaitingForStartResynchronize = 9,
        Resynchronizing = 10,
        ResynchronizeSuspended = 11
    }

    //public enum ReplicationState
    //{
    //    NA = 0,
    //    ReadyForInitialReplication = 1,
    //    WaitingForInitialReplication = 2,
    //    Replicating = 3,
    //    SyncedReplicationComplete = 4,
    //    FailOverWaitingCompletion = 5,
    //    FailedOver = 6,
    //    Suspended = 7,
    //    Error = 8,
    //    WaitingForStartResynchronize = 9,
    //    Resynchronizing = 10,
    //    ResynchronizeSuspended = 11
    //}

    public enum ReplicationHealth
    {
        NA = 0,
        Normal = 1,
        Warning = 2,
        Critical = 3
    }
}
