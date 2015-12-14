// Guids.cs
// MUST match guids.h
using System;

namespace ARDG.CompareFiles
{
    static class GuidList
    {
        public const string guidCompareFilesPkgString = "159740ff-0300-42f2-b81a-efad5d95a3bd";
        public const string guidConfigureCompareFilesCmdSetString = "880dba77-21c0-4397-aced-147cf7426af7";
        public const string guidCompareFilesCmdSetString = "880dba77-21c0-4397-aced-147cf7426af8";
        public const string guidCompareFilesWebCmdSetString = "880dba77-21c1-4397-aced-147cf7426af9";
        public const string guidCompareFilesMultiCmdSetString = "880dba77-21c1-4397-aced-147cf7426afa";

        public static readonly Guid guidConfigureCompareFilesCmdSet = new Guid(guidConfigureCompareFilesCmdSetString);
        public static readonly Guid guidCompareFilesCmdSet = new Guid(guidCompareFilesCmdSetString);
        public static readonly Guid guidCompareFilesWebCmdSet = new Guid(guidCompareFilesWebCmdSetString);
        public static readonly Guid guidCompareFilesMultiCmdSet = new Guid(guidCompareFilesMultiCmdSetString);
    };
}