// ZipFile.Extract.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009 Dino Chiesa.
// All rights reserved.
//
// This code module is part of DotNetZip, a zipfile class library.
//
// ------------------------------------------------------------------
//
// This code is licensed under the Microsoft Public License.
// See the file License.txt for the license details.
// More info on: http://dotnetzip.codeplex.com
//
// ------------------------------------------------------------------
//
// last saved (in emacs):
// Time-stamp: <2011-July-31 14:45:18>
//
// ------------------------------------------------------------------
//
// This module defines the methods for Extract operations on zip files.
//
// ------------------------------------------------------------------
//


using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace Ionic.Zip
{

    public partial class ZipFile
    {

        /// <summary>
        /// Extracts all of the items in the zip archive, to the specified path in the
        /// filesystem.  The path can be relative or fully-qualified.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   This method will extract all entries in the <c>ZipFile</c> to the
        ///   specified path.
        /// </para>
        ///
        /// <para>
        ///   If an extraction of a file from the zip archive would overwrite an
        ///   existing file in the filesystem, the action taken is dictated by the
        ///   ExtractExistingFile property, which overrides any setting you may have
        ///   made on individual ZipEntry instances.  By default, if you have not
        ///   set that property on the <c>ZipFile</c> instance, the entry will not
        ///   be extracted, the existing file will not be overwritten and an
        ///   exception will be thrown. To change this, set the property, or use the
        ///   <see cref="ZipFile.ExtractAll(string,
        ///   Ionic.Zip.ExtractExistingFileAction)" /> overload that allows you to
        ///   specify an ExtractExistingFileAction parameter.
        /// </para>
        ///
        /// <para>
        ///   The action to take when an extract would overwrite an existing file
        ///   applies to all entries.  If you want to set this on a per-entry basis,
        ///   then you must use one of the <see
        ///   cref="ZipEntry.Extract()">ZipEntry.Extract</see> methods.
        /// </para>
        ///
        /// <para>
        ///   This method will send verbose output messages to the <see
        ///   cref="StatusMessageTextWriter"/>, if it is set on the <c>ZipFile</c>
        ///   instance.
        /// </para>
        ///
        /// <para>
        /// You may wish to take advantage of the <c>ExtractProgress</c> event.
        /// </para>
        ///
        /// <para>
        ///   About timestamps: When extracting a file entry from a zip archive, the
        ///   extracted file gets the last modified time of the entry as stored in
        ///   the archive. The archive may also store extended file timestamp
        ///   information, including last accessed and created times. If these are
        ///   present in the <c>ZipEntry</c>, then the extracted file will also get
        ///   these times.
        /// </para>
        ///
        /// <para>
        ///   A Directory entry is somewhat different. It will get the times as
        ///   described for a file entry, but, if there are file entries in the zip
        ///   archive that, when extracted, appear in the just-created directory,
        ///   then when those file entries are extracted, the last modified and last
        ///   accessed times of the directory will change, as a side effect.  The
        ///   result is that after an extraction of a directory and a number of
        ///   files within the directory, the last modified and last accessed
        ///   timestamps on the directory will reflect the time that the last file
        ///   was extracted into the directory, rather than the time stored in the
        ///   zip archive for the directory.
        /// </para>
        ///
        /// <para>
        ///   To compensate, when extracting an archive with <c>ExtractAll</c>,
        ///   DotNetZip will extract all the file and directory entries as described
        ///   above, but it will then make a second pass on the directories, and
        ///   reset the times on the directories to reflect what is stored in the
        ///   zip archive.
        /// </para>
        ///
        /// <para>
        ///   This compensation is performed only within the context of an
        ///   <c>ExtractAll</c>. If you call <c>ZipEntry.Extract</c> on a directory
        ///   entry, the timestamps on directory in the filesystem will reflect the
        ///   times stored in the zip.  If you then call <c>ZipEntry.Extract</c> on
        ///   a file entry, which is extracted into the directory, the timestamps on
        ///   the directory will be updated to the current time.
        /// </para>
        /// </remarks>
        ///
        /// <example>
        ///   This example extracts all the entries in a zip archive file, to the
        ///   specified target directory.  The extraction will overwrite any
        ///   existing files silently.
        ///
        /// <code>
        /// String TargetDirectory= "unpack";
        /// using(ZipFile zip= ZipFile.Read(ZipFileToExtract))
        /// {
        ///     zip.ExtractExistingFile= ExtractExistingFileAction.OverwriteSilently;
        ///     zip.ExtractAll(TargetDirectory);
        /// }
        /// </code>
        ///
        /// <code lang="VB">
        /// Dim TargetDirectory As String = "unpack"
        /// Using zip As ZipFile = ZipFile.Read(ZipFileToExtract)
        ///     zip.ExtractExistingFile= ExtractExistingFileAction.OverwriteSilently
        ///     zip.ExtractAll(TargetDirectory)
        /// End Using
        /// </code>
        /// </example>
        ///
        /// <seealso cref="Ionic.Zip.ZipFile.ExtractProgress"/>
        /// <seealso cref="Ionic.Zip.ZipFile.ExtractExistingFile"/>
        ///
        /// <param name="path">
        ///   The path to which the contents of the zipfile will be extracted.
        ///   The path can be relative or fully-qualified.
        /// </param>
        ///
        public void ExtractAll(string path, int numberOfThreads = 1)
        {
            _InternalExtractAll(path, true, numberOfThreads);
        }



        /// <summary>
        /// Extracts all of the items in the zip archive, to the specified path in the
        /// filesystem, using the specified behavior when extraction would overwrite an
        /// existing file.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        /// This method will extract all entries in the <c>ZipFile</c> to the specified
        /// path.  For an extraction that would overwrite an existing file, the behavior
        /// is dictated by <paramref name="extractExistingFile"/>, which overrides any
        /// setting you may have made on individual ZipEntry instances.
        /// </para>
        ///
        /// <para>
        /// The action to take when an extract would overwrite an existing file
        /// applies to all entries.  If you want to set this on a per-entry basis,
        /// then you must use <see cref="ZipEntry.Extract(String,
        /// ExtractExistingFileAction)" /> or one of the similar methods.
        /// </para>
        ///
        /// <para>
        /// Calling this method is equivalent to setting the <see
        /// cref="ExtractExistingFile"/> property and then calling <see
        /// cref="ExtractAll(String)"/>.
        /// </para>
        ///
        /// <para>
        /// This method will send verbose output messages to the
        /// <see cref="StatusMessageTextWriter"/>, if it is set on the <c>ZipFile</c> instance.
        /// </para>
        /// </remarks>
        ///
        /// <example>
        /// This example extracts all the entries in a zip archive file, to the
        /// specified target directory.  It does not overwrite any existing files.
        /// <code>
        /// String TargetDirectory= "c:\\unpack";
        /// using(ZipFile zip= ZipFile.Read(ZipFileToExtract))
        /// {
        ///   zip.ExtractAll(TargetDirectory, ExtractExistingFileAction.DontOverwrite);
        /// }
        /// </code>
        ///
        /// <code lang="VB">
        /// Dim TargetDirectory As String = "c:\unpack"
        /// Using zip As ZipFile = ZipFile.Read(ZipFileToExtract)
        ///     zip.ExtractAll(TargetDirectory, ExtractExistingFileAction.DontOverwrite)
        /// End Using
        /// </code>
        /// </example>
        ///
        /// <param name="path">
        /// The path to which the contents of the zipfile will be extracted.
        /// The path can be relative or fully-qualified.
        /// </param>
        ///
        /// <param name="extractExistingFile">
        /// The action to take if extraction would overwrite an existing file.
        /// </param>
        /// <seealso cref="ExtractSelectedEntries(String,ExtractExistingFileAction)"/>
        public void ExtractAll(string path, ExtractExistingFileAction extractExistingFile, int numberOfThreads = 1)
        {
            ExtractExistingFile = extractExistingFile;
            _InternalExtractAll(path, true, numberOfThreads);
        }


        private void _InternalExtractAll(string path, bool overrideExtractExistingProperty, int numberOfThreads = 1)
        {
            bool header = Verbose;
            _inExtractAll = true;
            try
            {
                OnExtractAllStarted(path);

                int n = 0;
                if (numberOfThreads > 1)
                {
                    LockFreeQueue<ZipEntry> work = new LockFreeQueue<ZipEntry>();
                    foreach(ZipEntry e in _entries.Values)
                    {
                        work.Enqueue(e);
                    }
                    List<Thread> workers = new List<Thread>();
                    for (int i = 0; i < numberOfThreads; i++)
                    {
                        workers.Add(new Thread(new ThreadStart(delegate
                        {
                            using (var viewStream = new FileStream(Name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                while (true)
                                {
                                    ZipEntry e = work.Dequeue();
                                    if (e != null)
                                    {
                                        int entryIndex = Interlocked.Increment(ref n) - 1;
                                        FileInfo outputFile = new FileInfo(Path.Combine(path, e.FileName));
                                        if (!outputFile.Directory.Exists)
                                        {
                                            outputFile.Directory.Create();
                                        }

                                        if (header)
                                        {
                                            StatusMessageTextWriter.WriteLine("\n{1,-22} {2,-8} {3,4}   {4,-8}  {0}",
                                                      "Name", "Modified", "Size", "Ratio", "Packed");
                                            StatusMessageTextWriter.WriteLine(new System.String('-', 72));
                                            header = false;
                                        }
                                        if (Verbose)
                                        {
                                            StatusMessageTextWriter.WriteLine("{1,-22} {2,-8} {3,4:F0}%   {4,-8} {0}",
                                                      e.FileName,
                                                      e.LastModified.ToString("yyyy-MM-dd HH:mm:ss"),
                                                      e.UncompressedSize,
                                                      e.CompressionRatio,
                                                      e.CompressedSize);
                                            if (!String.IsNullOrEmpty(e.Comment))
                                                StatusMessageTextWriter.WriteLine("  Comment: {0}", e.Comment);
                                        }
                                        e.Password = _Password;  // this may be null
                                        OnExtractEntry(entryIndex, true, e, path);
                                        if (overrideExtractExistingProperty)
                                        e.ExtractExistingFile = this.ExtractExistingFile;
                                        
                                        bool extractFile = true;
                                        if (outputFile.Exists)
                                        {
                                                int rc = e.CheckExtractExistingFile(path, outputFile.FullName);
                                                if (rc == 2) return; // task canceled. //TODO: this will only cancel one of the threads, should cancel the entire operation
                                                if (rc == 1) extractFile = false; // do not overwrite
                                        }   
                                        if (extractFile)
                                        {
                                            using (FileStream fs = new FileStream(outputFile.FullName, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                                            {
                                                fs.SetLength(e.UncompressedSize);

                                                e.Extract(fs, viewStream);
                                            }
                                        }

                                        OnExtractEntry(entryIndex + 1, false, e, path);
                                        if (_extractOperationCanceled)
                                            return;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        })));
                    }

                    foreach(var worker in workers)
                    {
                        worker.IsBackground = true;
                        worker.Start();
                    }

                    foreach(var worker in workers)
                    {
                        worker.Join();
                    }
                }
                else
                {
                    foreach (ZipEntry e in _entries.Values)
                    {
                        if (header)
                        {
                            StatusMessageTextWriter.WriteLine("\n{1,-22} {2,-8} {3,4}   {4,-8}  {0}",
                                      "Name", "Modified", "Size", "Ratio", "Packed");
                            StatusMessageTextWriter.WriteLine(new System.String('-', 72));
                            header = false;
                        }
                        if (Verbose)
                        {
                            StatusMessageTextWriter.WriteLine("{1,-22} {2,-8} {3,4:F0}%   {4,-8} {0}",
                                      e.FileName,
                                      e.LastModified.ToString("yyyy-MM-dd HH:mm:ss"),
                                      e.UncompressedSize,
                                      e.CompressionRatio,
                                      e.CompressedSize);
                            if (!String.IsNullOrEmpty(e.Comment))
                                StatusMessageTextWriter.WriteLine("  Comment: {0}", e.Comment);
                        }
                        e.Password = _Password;  // this may be null
                        OnExtractEntry(n, true, e, path);
                        if (overrideExtractExistingProperty)
                            e.ExtractExistingFile = this.ExtractExistingFile;
                        e.Extract(path);
                        n++;
                        OnExtractEntry(n, false, e, path);
                        if (_extractOperationCanceled)
                            break;
                    }
                }

                if (!_extractOperationCanceled)
                {
                    // workitem 8264:
                    // now, set times on directory entries, again.
                    // The problem is, extracting a file changes the times on the parent
                    // directory.  So after all files have been extracted, we have to
                    // run through the directories again.
                    foreach (ZipEntry e in _entries.Values)
                    {
                        // check if it is a directory
                        if ((e.IsDirectory) || (e.FileName.EndsWith("/")))
                        {
                            string outputFile = (e.FileName.StartsWith("/"))
                                ? Path.Combine(path, e.FileName.Substring(1))
                                : Path.Combine(path, e.FileName);

                            e._SetTimes(outputFile, false);
                        }
                    }
                    OnExtractAllCompleted(path);
                }

            }
            finally
            {

                _inExtractAll = false;
            }
        }


    }
}
