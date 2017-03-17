﻿using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;

namespace Hermex.Classes.Uploaders
{
    public class SocketUploader
    {

        private enum Messages
        {
            OK,
            WRONG_PASSWORD,
            FILE_NOT_RECOGNIZED,
            FILE_TOO_LARGE,
            SERVER_FULL,
            UNKNOWN_ERROR
        }

        private TcpClient socket;
        private FileInfo file;
        private string _link;
        private string _fileName;

        private IProgress<int> progressReporter;

        public SocketUploader(FileInfo f, Progress<int> pReporter, string sendname)
        {
            file = f;
            progressReporter = pReporter;
            FilenameToSend = sendname;
        }

        public bool Upload()
        {
            socket = new TcpClient(AppSettings.Get<string>("SocketAddress"), AppSettings.Get<int>("SocketPort"));

            DataOutputStream output = new DataOutputStream(new BinaryWriter(socket.GetStream()));
            DataInputStream input = new DataInputStream(new BinaryReader(socket.GetStream()));

            // password & file lenght & type
            output.WriteUTF(AppSettings.Get<string>("SocketPassword") + "&" + file.Length + "&" + FilenameToSend);

            if(input.ReadUTF().Equals(Messages.OK.ToString()))
            {

                using(FileStream fileReader = new FileStream(file.FullName, FileMode.Open))
                {
                    int bufferSize = (int)Math.Min(4096, file.Length);
                    byte[] buffer = new byte[bufferSize];
                    int sentBytes = 0;

                    int progress = 0;

                    while(sentBytes < file.Length)
                    {
                        int readed = fileReader.Read(buffer, 0, bufferSize);
                        socket.GetStream().Write(buffer, 0, readed);

                        sentBytes += readed;
                        progress = (int) (100 * sentBytes / file.Length);

                        progressReporter.Report(progress);
                    }
                }
                

                Link = input.ReadUTF();
                socket.Close();

                return true;
            }
            

            if(input.ReadUTF().Equals(Messages.WRONG_PASSWORD.ToString()))
            {
                //display wrong password
            }

            if(input.ReadUTF().Equals(Messages.FILE_NOT_RECOGNIZED.ToString()))
            {
                //server doesn't recognize file type
            }

            if(input.ReadUTF().Equals(Messages.FILE_TOO_LARGE.ToString()))
            {
                //file too large
            }

            if(input.ReadUTF().Equals(Messages.SERVER_FULL.ToString()))
            {
                //server not have space
            }

            if(input.ReadUTF().Equals(Messages.UNKNOWN_ERROR.ToString()))
            {
                //unknow error happened
            }
            return false;
        }

        public void Stop()
        {
            socket.Close();
        }

        public string Link
        {
            get
            {
                return _link;
            }

            private set
            {
                _link = value;
            }
        }
        public string FilenameToSend
        {
            get
            {
                return _fileName;
            }

            private set
            {
                _fileName = value;
            }
        }
    }
}