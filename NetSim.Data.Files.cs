using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace NetSim.Data.Files
{
    /// <summary>
    /// Creates a text file for writing
    /// </summary>
    public class OutputFile : FileStream
    {
        /// <summary>
        /// the streamwriter
        /// </summary>
        private StreamWriter sw { get; set; }

        /// <summary>
        /// Creates a text file for writing
        /// </summary>
        /// <param name="fileName">name of the file</param>
        public OutputFile(String fileName)
            : base(OutputFile.CheckAndGetFileName(fileName), FileMode.OpenOrCreate, FileAccess.Write)
        {
            this.sw = new StreamWriter(this);
        }

        /// <summary>
        /// Writes matrix data into file with given format for each number
        /// </summary>
        /// <typeparam name="T">Type of the data in the matrix</typeparam>
        /// <param name="data">the columns of the data matrix, each of wich should be entered as a different parameter for this method</param>
        /// <param name="format">format for writing data from the matrix</param>
        /// <param name="separator">column separator character</param>
        /// <param name="header">header of the file</param>
        /// <param name="transpose">transpose data matrix before writing</param>
        public void WriteData<T>(String format, String separator, String header, Boolean transpose, params T[][] data)
        {
            format = "{0:" + format + "}";
            if (header != String.Empty)
            {
                this.WriteLine(header);
            }

            if (transpose)
            {
                data = this.Transpose(data);

            }

            Int32 n = data[0].Length;

            if (data.Any(e => e.Length != n))
            {
                throw new ArgumentOutOfRangeException("All data vectors supplied to this method must have the same length!");
            }
            
            Int32 j, i = 0, nCol = data.Length - 1;
            while (i < n)
            {
                j = 0;
                while (j < nCol)
                {
                    this.Write(format + separator, data[j][i]);
                    j++;
                }
                this.Write(format + Environment.NewLine, data[nCol][i]);
                i++;
            }
        }

        private T[][] Transpose<T>(T[][] A)
        {
            Int32 i = 0, j, n = A.Length, m = A[0].Length;
            T[][] B = new T[m][];
            while (i < m)
            {
                B[i] = new T[n];
                i++;
            }
            i = 0;
            while (i < n)
            {
                j = 0;
                while (j < m)
                {
                    B[j][i] = A[i][j];
                    j++;
                }
                i++;
            }
            return B;
        }

        /// <summary>
        /// writes formatted string
        /// </summary>
        /// <param name="format">format of the string</param>
        /// <param name="args">arguments (does not need an array)</param>
        public void WriteLine(String format, params Object[] args)
        {
            this.sw.WriteLine(format, args);
        }

        /// <summary>
        /// writes text to the file
        /// </summary>
        /// <param name="text">text to be written</param>
        public void WriteLine(String text)
        {
            this.sw.WriteLine(text);
        }

        /// <summary>
        /// writes formatted string to the file without breaking line
        /// </summary>
        /// <param name="format">format string</param>
        /// <param name="args">arguments (does not need an array)</param>
        public void Write(String format, params Object[] args)
        {
            this.sw.Write(format, args);
        }

        /// <summary>
        /// writes text to the file without breaking line
        /// </summary>
        /// <param name="text">text to be written</param>
        public void Write(String text)
        {
            this.sw.Write(text);
        }

        /// <summary>
        /// closes file and streamwriter
        /// </summary>
        public new void Close()
        {
            this.sw.Close();
            base.Close();
        }

        /// <summary>
        /// checks if filename is available in the directory it was passed and returns a free filename
        /// </summary>
        /// <param name="fileNameWithExtension">complete filename (relative or absolute)</param>
        /// <returns>a free filename based on the passed one</returns>
        public static String CheckAndGetFileName(String fileNameWithExtension)
        {
            return OutputFile.CheckAndGetFileName(Path.GetFileNameWithoutExtension(fileNameWithExtension), Path.GetExtension(fileNameWithExtension).Substring(1));
        }

        /// <summary>
        ///  checks if filename is available in the directory it was passed and returns a free filename
        /// </summary>
        /// <param name="fileName">no extension filename (relative or absolute)</param>
        /// <param name="ext">file extension</param>
        /// <returns>a free filename based on the passed one</returns>
        private static String CheckAndGetFileName(String fileName, String ext)
        {
            // checking whether filename already exists
            String patternStr1 = "^" + fileName.Replace(Path.DirectorySeparatorChar, '%').Replace('/', '%') + @"_*[0-9]*\." + ext + @"$";
            String patternStr2 = fileName.Replace(Path.DirectorySeparatorChar, '%').Replace('/', '%') + @"_*[0-9]*\." + ext + @"$";
            String temp;
            String dir = Path.GetDirectoryName(fileName);
            dir = (dir == "" ? Directory.GetCurrentDirectory() : dir);
            List<String> currFilesList = Directory.GetFiles(dir, "*" + ext, SearchOption.TopDirectoryOnly).ToList<String>();//.Select(e => e.Replace(Path.DirectorySeparatorChar, '%')).ToList<String>();
            Int32 i = 0;
            while (i < currFilesList.Count)
            {
                currFilesList[i] = currFilesList[i].Replace(Path.DirectorySeparatorChar, '%').Replace('/', '%');
                currFilesList[i] = System.Text.RegularExpressions.Regex.Match(currFilesList[i], patternStr2, System.Text.RegularExpressions.RegexOptions.Compiled).Value;
                i++;
            }
            try
            {
                i = 0;
                while (true)
                {
                    temp = currFilesList.First<String>(s => System.Text.RegularExpressions.Regex.IsMatch(s, patternStr1, System.Text.RegularExpressions.RegexOptions.Compiled));
                    if (!String.IsNullOrEmpty(temp))
                    {
                        currFilesList.Remove(temp);
                        i++;
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (InvalidOperationException)
            {
                temp = (i == 0 ? String.Empty : "_" + i.ToString());
                fileName = fileName + temp + "." + ext;
            }
            return fileName;
        }
    }

    /// <summary>
    /// opens a text file for reading
    /// </summary>
    public class InputFile : FileStream
    {
        /// <summary>
        /// the streamreader
        /// </summary>
        private StreamReader sr { get; set; }

        /// <summary>
        /// indicates whether the file is at the end of the stream
        /// </summary>
        public Boolean EndOfStream
        {
            get
            {
                return this.sr.EndOfStream;
            }
        }

        /// <summary>
        /// reads a line from the file
        /// </summary>
        public Func<String> ReadLine;

        /// <summary>
        /// opens a text file for reading
        /// </summary>
        /// <param name="fileName">the name of the file</param>
        public InputFile(String fileName)
            : base(fileName, FileMode.Open, FileAccess.Read)
        {
            this.sr = new StreamReader(this);
            this.ReadLine = this.sr.ReadLine;
        }
        
        /// <summary>
        /// closes the file and the streamreader
        /// </summary>
        public new void Close()
        {
            this.sr.Close();
            base.Close();
        }
    }
}