using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Management.Automation;
using System.Security.Cryptography;
using System.IO.Compression;
using System.IO;

namespace PwshCode
{
    /// <summary>
    /// <para type="synopsis">Put anything in, and out you get gibberish? PwshCode? Who knows..</para>
    /// <para type="description">Encrypt string or objects, to a funny an customizable format. Not very effecient, and output is quite bloated, so this is not for production use.. Idea for this came from a few esoteric languages.. At least, evildoers must have great skills, and too much time on their hand, to be able to crack this. For top security, specify the Secret parameter, and if very bloated, change prefix to none.</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "PwshCode")]
    [OutputType(typeof(System.String))]
    public class GetPwshCode : PSCmdlet
    {
        [Parameter(Mandatory = true,
            ValueFromPipeline = true,
            Position = 0)]
        [ValidateNotNullOrEmpty()]
        public PSObject InputObject { get; set; }

        [Parameter(Position = 1)]
        [ValidateNotNullOrEmpty()]
        public string Secret { get; set; }

        [Parameter(Position = 2)]
        [ValidateSet("Pwsh", "Posh", "PS", "Ook", "None")]
        public string Prefix { get; set; } = "Pwsh";

        protected override void ProcessRecord()
        {
            PwshCode pwsh = new PwshCode();
            string ObjectType = InputObject.ImmediateBaseObject.GetType().FullName;
            string Input;

            switch (ObjectType)
            {
                case "System.String":
                    Input = InputObject.ToString() + "##IsString##";
                    break;
                default:
                    Input = PSSerializer.Serialize(InputObject);
                    break;
            }

            Prefix = Prefix == "None" ? "" : Prefix;

            string pwshCode = String.IsNullOrEmpty(Secret) ? pwsh.GetPwshCode(Input, Prefix) : pwsh.GetPwshCode(Input, Secret, Prefix);
            
            WriteObject(pwshCode);
        }
    }

    /// <summary>
    /// <para type="synopsis">Put PwshCode in, and maybe you get something valuable out?</para>
    /// <para type="description">Decrypt string or objects, from a funny an customizable format. Idea for this came from a few esoteric languages.. At least, evildoers must have great skills, and too much time on their hand, to be able to crack this. The secret and prefix must match what was specified when creating the PwshCode.</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Undo, "PwshCode")]
    [OutputType(typeof(System.String))]
    public class UndoPwshCode : PSCmdlet
    {
        [Parameter(Mandatory = true,
            ValueFromPipeline = true,
            Position = 0)]
        public string PwshCode { get; set; }

        [Parameter(Position = 1)]
        public string Secret { get; set; }

        [Parameter(Position = 2)]
        [ValidateSet("Pwsh", "Posh", "PS", "Ook", "None")]
        public string Prefix { get; set; } = "Pwsh";

        protected override void ProcessRecord()
        {
            PwshCode pwsh = new PwshCode();
            try
            {
                Prefix = Prefix == "None" ? "" : Prefix;
                string Output = String.IsNullOrEmpty(Secret) ? pwsh.UndoPwshCode(PwshCode, Prefix) : pwsh.UndoPwshCode(PwshCode, Secret, Prefix);
                if (!Output.Contains("##IsString##"))
                {
                    PSObject DeserializedOutput = PSObject.AsPSObject(PSSerializer.Deserialize(Output));
                    WriteObject(DeserializedOutput);
                }
                else
                {
                    WriteObject(Output.Replace("##IsString##", ""));
                }
            }
            catch
            {
                WriteObject("Don't think so! ;)");
            }
}
    }

    class PwshCode
    {
        public string GetPwshCode(string Input, string Secret, string Prefix)
        {
            string CipherText = EncryptData(Input, Secret);
            string CompressedCipherText = CompressString(CipherText);
            string Brainfuck = GetBrainFuck(CompressedCipherText);
            string pwshCode = _GetPwshCode(Brainfuck, Prefix);
            return pwshCode;
        }

        public string GetPwshCode(string Input, string Prefix)
        {
            string Brainfuck = GetBrainFuck(Input);
            string pwshCode = _GetPwshCode(Brainfuck, Prefix);
            return pwshCode;
        }

        public string UndoPwshCode(string PwshCode, string Secret, string Prefix)
        {
            string Brainfuck = _UndoPwshCode(PwshCode.Trim(), Prefix);
            string CompressedCipherText = UndoBrainFuck(Brainfuck);
            string CipherText = DecompressString(CompressedCipherText);
            string ClearText = DecryptData(CipherText, Secret);
            return ClearText;
        }

        public string UndoPwshCode(string PwshCode, string Prefix)
        {
            string Brainfuck = _UndoPwshCode(PwshCode.Trim(), Prefix);
            string ClearText = UndoBrainFuck(Brainfuck);
            return ClearText;
        }

        private string GetBrainFuck(string Text)
        {
            Byte[] Letters = GetByte(Text);
            int Value = 0;
            string Result = "";

            foreach (Byte Letter in Letters)
            {
                int Diff = Letter - Value;

                Value = Letter;

                if (Diff == 0)
                {
                    Result += ">";
                }

                if (Math.Abs(Diff) < 10)
                {
                    if (Diff > 0)
                    {
                        Result += ">" + string.Concat(Enumerable.Repeat("+", Diff));
                    }
                    else if (Diff < 0)
                    {
                        Result += ">" + string.Concat(Enumerable.Repeat("-", Math.Abs(Diff)));
                    }
                }
                else
                {
                    int Loop = Convert.ToInt32(Math.Floor(Math.Sqrt(Math.Abs(Diff))));

                    Result += string.Concat(Enumerable.Repeat("+", Loop));

                    if (Diff >= 0)
                    {
                        Result += "[->" + string.Concat(Enumerable.Repeat("+", Loop)) + "<]";
                        Result += ">" + string.Concat(Enumerable.Repeat("+", Diff - Convert.ToInt32(Math.Pow(Convert.ToDouble(Loop), 2))));
                    }
                    else if (Diff < 0)
                    {
                        Result += "[->" + string.Concat(Enumerable.Repeat("-", Loop)) + "<]";
                        Result += ">" + string.Concat(Enumerable.Repeat("-", Math.Abs(Diff) - Convert.ToInt32(Math.Pow(Convert.ToDouble(Loop), 2))));
                    }
                }
                Result += ".<";
            }
            return Result.Replace("<>", "");
        }

        private Byte[] GetByte(string Text)
        {
            return Encoding.UTF8.GetBytes(Text);
        }

        private string _GetPwshCode(string Brainfuck, string Prefix)
        {
            Dictionary<char, string> Translation = new Dictionary<char, string>()
            {
                { '>', "Prefix. Prefix?" },
                { '<', "Prefix? Prefix." },
                { '+', "Prefix. Prefix." },
                { '-', "Prefix! Prefix!" },
                { '.', "Prefix! Prefix." },
                { ',', "Prefix. Prefix!" },
                { '[', "Prefix! Prefix?" },
                { ']', "Prefix? Prefix!" }
            };

            char[] Letters = Encoding.UTF8.GetChars(Encoding.UTF8.GetBytes(Brainfuck));
            StringBuilder Translated = new StringBuilder();

            int Counter = 1;
            foreach (Char Letter in Letters)
            {
                Translated.Append(Translation[Letter].Replace("Prefix", Prefix) + " ");
                Counter++;
            }

            return Translated.ToString().Trim();
        }

        private string _UndoPwshCode(string PwshCode, string Prefix)
        {
            Dictionary<string, char> Translation = new Dictionary<string, char>()
            {
                { (Prefix + ". " + Prefix + "?"), '>' },
                { (Prefix + "? " + Prefix + "."), '<' },
                { (Prefix + ". " + Prefix + "."), '+' },
                { (Prefix + "! " + Prefix + "!"), '-' },
                { (Prefix + "! " + Prefix + "."), '.' },
                { (Prefix + ". " + Prefix + "!"), ',' },
                { (Prefix + "! " + Prefix + "?"), '[' },
                { (Prefix + "? " + Prefix + "!"), ']' }
            };

            string[] Commands = PwshCode.Trim().Replace("Prefix", Prefix).Split(' ');
            StringBuilder Translated = new StringBuilder();

            for (int i = 0; i < Commands.Length; i += 2)
            {
                Translated.Append(Translation[Commands[i] + " " + Commands[i + 1]]);
            }

            return Translated.ToString().Trim();
        }

        private string UndoBrainFuck(string BrainFuck)
        {
            int i = 0;
            int right = BrainFuck.Length;
            int ptr = 0;
            int bufferSize = 65535;
            int[] buffer = new int[bufferSize];
            StringBuilder Output = new StringBuilder();

            while (i < right)
            {
                switch (BrainFuck[i])
                {
                    case '>':
                        {
                            ptr++;
                            if (ptr >= bufferSize)
                            {
                                ptr = 0;
                            }
                            break;
                        }
                    case '<':
                        {
                            ptr--;
                            if (ptr < 0)
                            {
                                ptr = bufferSize - 1;
                            }
                            break;
                        }
                    case '.':
                        {
                            Output.Append((char)buffer[ptr]);
                            break;
                        }
                    case '+':
                        {
                            buffer[ptr]++;
                            break;
                        }
                    case '-':
                        {
                            buffer[ptr]--;
                            break;
                        }
                    case '[':
                        {
                            if (buffer[ptr] == 0)
                            {
                                int loop = 1;
                                while (loop > 0)
                                {
                                    i++;
                                    char c = BrainFuck[i];
                                    if (c == '[')
                                    {
                                        loop++;
                                    }
                                    else
                                    if (c == ']')
                                    {
                                        loop--;
                                    }
                                }
                            }
                            break;
                        }
                    case ']':
                        {
                            int loop = 1;
                            while (loop > 0)
                            {
                                i--;
                                char c = BrainFuck[i];
                                if (c == '[')
                                {
                                    loop--;
                                }
                                else
                                if (c == ']')
                                {
                                    loop++;
                                }
                            }
                            i--;
                            break;
                        }
                    case ',':
                        {
                            Output.Append(buffer[ptr]);
                            break;
                        }
                }
                i++;
            }

            return Output.ToString();
        }

        private string EncryptData(string textData, string Encryptionkey)
        {
            RijndaelManaged objrij = new RijndaelManaged
            {

                //set the mode for operation of the algorithm
                Mode = CipherMode.CBC,

                //set the padding mode used in the algorithm.
                Padding = PaddingMode.PKCS7,

                //set the size, in bits, for the secret key.
                KeySize = 0x80,

                //set the block size in bits for the cryptographic operation.
                BlockSize = 0x80
            };

            //set the symmetric key that is used for encryption & decryption.
            byte[] passBytes = Encoding.UTF8.GetBytes(Encryptionkey);

            //set the initialization vector (IV) for the symmetric algorithm
            byte[] EncryptionkeyBytes = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            int len = passBytes.Length;

            if (len > EncryptionkeyBytes.Length)
            {
                len = EncryptionkeyBytes.Length;
            }

            Array.Copy(passBytes, EncryptionkeyBytes, len);

            objrij.Key = EncryptionkeyBytes;

            objrij.IV = EncryptionkeyBytes;

            //Creates symmetric AES object with the current key and initialization vector IV.
            ICryptoTransform objtransform = objrij.CreateEncryptor();

            byte[] textDataByte = Encoding.UTF8.GetBytes(textData);

            objrij.Dispose();

            //Final transform the test string.
            return Convert.ToBase64String(objtransform.TransformFinalBlock(textDataByte, 0, textDataByte.Length));
        }

        private string DecryptData(string EncryptedText, string Encryptionkey)
        {
            RijndaelManaged objrij = new RijndaelManaged
            {
                Mode = CipherMode.CBC,

                Padding = PaddingMode.PKCS7,

                KeySize = 0x80,

                BlockSize = 0x80
            };

            byte[] encryptedTextByte = Convert.FromBase64String(EncryptedText);

            byte[] passBytes = Encoding.UTF8.GetBytes(Encryptionkey);

            byte[] EncryptionkeyBytes = new byte[0x10];

            int len = passBytes.Length;

            if (len > EncryptionkeyBytes.Length)
            {
                len = EncryptionkeyBytes.Length;
            }

            Array.Copy(passBytes, EncryptionkeyBytes, len);

            objrij.Key = EncryptionkeyBytes;

            objrij.IV = EncryptionkeyBytes;

            byte[] TextByte = objrij.CreateDecryptor().TransformFinalBlock(encryptedTextByte, 0, encryptedTextByte.Length);

            objrij.Dispose();

            return Encoding.UTF8.GetString(TextByte);  //it will return readable string

        }

        private string CompressString(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            var memoryStream = new MemoryStream();
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gZipStream.Write(buffer, 0, buffer.Length);
            }

            memoryStream.Position = 0;

            var compressedData = new byte[memoryStream.Length];
            memoryStream.Read(compressedData, 0, compressedData.Length);

            var gZipBuffer = new byte[compressedData.Length + 4];
            Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
            return Convert.ToBase64String(gZipBuffer);
        }

        private string DecompressString(string compressedText)
        {
            byte[] gZipBuffer = Convert.FromBase64String(compressedText);
            using (var memoryStream = new MemoryStream())
            {
                int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

                var buffer = new byte[dataLength];

                memoryStream.Position = 0;
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }
    }
}
