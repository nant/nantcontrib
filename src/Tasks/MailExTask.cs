//
// NAntContrib
// Copyright (C) 2002 Tomas Restrepo (tomasr@mvps.org)
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307 USA
//


using System;
using System.Text;
using System.Web.Mail;
using System.IO;
using SourceForge.NAnt.Attributes;
using SourceForge.NAnt;

namespace NAnt.Contrib.Tasks 
{ 

   /// <summary>
   /// A task to send SMTP email. Based on Geri Shaw's MailTask for NAnt.
   /// </summary>
   /// <remarks>
   /// You can specify either a hard-coded message text,
   /// as well as a file to use as the body. Content can be
   /// either plain text or html.
   /// You can also attach files.
   /// </remarks>
   /// <example>
   ///   <para>Sends an html-formatted email from nant@sourceforge.net to three recipients with a subject about the attachments.  The body of the message will be the combined contents of body1.txt through body4.txt.  The body1.txt through body3.txt files will also be included as attachments.  The message will be sent using the smtpserver.anywhere.com SMTP server.</para>
   ///   <code><![CDATA[
   ///     <mailex
   ///       from="nAnt@sourceforge.net" 
   ///       tolist="recipient1@sourceforge.net" 
   ///       cclist="recipient2@sourceforge.net" 
   ///       bcclist="recipient3@sourceforge.net" 
   ///       subject="Msg 7: With attachments" 
   ///       format="Html"
   ///       file="body1.html" 
   ///       mailhost="smtpserver.anywhere.com">
   ///       <attachments>
   ///         <includes name="*.txt"/>
   ///       </attachments>
   ///     </mailex>
   ///   ]]></code>
   /// </example>
   [TaskName("mailex")]
   public class MailExTask : Task
   {
        
      string _from = "";      
      string _toList = "";        
      string _ccList = "";        
      string _bccList = "";        
      string _mailHost = "localhost";
      string _subject = "";        
      string _message = null;        
      string _file = null;        
      FileSet _attachments = new FileSet();
      MailFormat _format = MailFormat.Text;
  
      /// <summary>
      /// Email address of sender
      /// </summary>
      [TaskAttribute("from", Required=true)]
      public string From { 
         get { return _from; } 
         set { _from = value; } 
      }
        
      /// <summary>
      /// Comma- or semicolon-separated list of 
      /// recipient email addresses
      /// </summary>
      [TaskAttribute("tolist", Required=true)]
      public string ToList {
         // convert to semicolon delimited
         get { return _toList.Replace("," , ";"); }
         set { _toList = value.Replace("," , ";"); }
      }

      /// <summary>
      /// Comma- or semicolon-separated list of CC: 
      /// recipient email addresses 
      /// </summary>
      [TaskAttribute("cclist")]
      public string CcList { 
         // convert to semicolon delimited
         get { return _ccList.Replace("," , ";"); } 
         set { _ccList = value.Replace("," , ";"); }
      }

      /// <summary> 
      /// Comma- or semicolon-separated list of BCC: 
      /// recipient email addresses
      /// </summary>
      [TaskAttribute("bcclist")]
      public string BccList { 
         // convert to semicolon delimited
         get { return _bccList.Replace("," , ";"); } 
         set { _bccList = value.Replace("," , ";"); }
      }

      /// <summary>
      /// Host name of mail server. Defaults to "localhost"
      /// </summary>
      [TaskAttribute("mailhost")]
      public string Mailhost { 
         get { return _mailHost; } 
         set { _mailHost = value; }
      }
  
      /// <summary>
      /// Text to send in body of email message.
      /// </summary>
      [TaskAttribute("message")]
      public string Message { 
         get { return _message; } 
         set { _message = value; } 
      }

      /// <summary>
      /// Text to send in subject line of email message.
      /// </summary>
      [TaskAttribute("subject")]
      public string Subject { 
         get { return _subject; } 
         set { _subject = value; } 
      }

      /// <summary>
      /// Format of the message body.
      /// </summary>
      [TaskAttribute("format")]
      public string Format { 
         get { return _format.ToString(); } 
         set { _format = (MailFormat)Enum.Parse(typeof(MailFormat), value); } 
      }

      /// <summary>
      /// Name of file to send as the body of the email message. 
      /// The file and message attributes are mutually exclusive
      /// </summary>
      [TaskAttribute("file")]
      public string Files { 
         // convert to semicolon delimited
         get { return _file; } 
         set { _file = value; }
      }

      /// <summary>
      /// Name(s) of files to send as attachments to email message.
      /// Multiple file names are comma- or semicolon-separated.
      /// </summary>
      [FileSet("attachments")]
      public FileSet Attachments { 
         // convert to semicolon delimited
         get { return _attachments; } 
      }

      ///<summary>
      ///Initializes task and ensures the 
      ///supplied attributes are valid.
      ///</summary>
      ///<param name="taskNode">Xml node used to define this task instance.</param>
      protected override void InitializeTask(System.Xml.XmlNode taskNode) 
      {

         if (From.Length == 0) {
            throw new BuildException("Mail attribute \"from\" is required.", Location);
         }

         if (ToList.Length == 0 && CcList.Length == 0 && BccList.Length == 0) {
            throw new BuildException("Mail must provide at least one of these attributes: \"tolist\", \"cclist\" or \"bcclist\".", Location);
         }

      }

      /// <summary>
      /// This is where the work is done
      /// </summary>
      protected override void ExecuteTask() 
      {

         MailMessage mailMessage = new MailMessage();
            
         mailMessage.From = this.From;
         mailMessage.To = this.ToList;
         mailMessage.Bcc = this.BccList;
         mailMessage.Cc = this.CcList;
         mailMessage.Subject = this.Subject;
         mailMessage.BodyFormat = this._format;

         //
         // you can't have file and message both
         //
         if ( (_message != null) && (_file != null) )
            throw new BuildException("You cannot specify both \"file\" and \"message\"");

         if ( _message != null ) {
            mailMessage.Body = _message;
         } else if ( _file != null ) {
            string content = ReadFile(_file);
            mailMessage.Body = content;
         }

         // Append file(s) to message body
         foreach ( string file in Attachments.FileNames ) 
         {
            try {
               // MailAttachment likes fully-qualified file names, use FileInfo to get them
               FileInfo fileInfo = new FileInfo(file);
               MailAttachment attach = new MailAttachment(fileInfo.FullName);
               mailMessage.Attachments.Add(attach);
            } catch {
               string msg = "WARNING! File \"" + file + "\" NOT attached to message. File does not exist or cannot be accessed. Check: " + Location.ToString() + "attachments=\"" + Attachments + "\"";
               Log.WriteLine(LogPrefix + msg);
            }
         }

         // send message
         try {
            SmtpMail.SmtpServer = this.Mailhost;
            SmtpMail.Send(mailMessage);
         } catch (Exception e) {
            StringBuilder msg = new StringBuilder();
            msg.Append(LogPrefix + "Error enountered while sending mail message.\n");
            msg.Append(LogPrefix + "Make sure that mailhost=" + this.Mailhost + " is valid\n");
            msg.Append(LogPrefix + "Internal Message: " + e.Message + "\n");
            msg.Append(LogPrefix + "Stack Trace:\n");
            msg.Append(e.ToString() + "\n");
            throw new BuildException("Error sending mail:\n " + msg.ToString());
         }
      }

      /// <summary>
      /// Reads a text file and returns the contents
      /// in a string
      /// </summary>
      /// <param name="filename"></param>
      /// <returns></returns>
      private string ReadFile(string filename)
      {
         StreamReader reader = null;
         try {
            reader = new StreamReader(File.OpenRead(filename));
            return reader.ReadToEnd();
         } catch {
            string msg = "WARNING! File \"" + filename + "\" NOT added to message body. File does not exist or is not readable. Check: " + Location.ToString() + "files=\"" + Files + "\"";
            Log.WriteLine(LogPrefix + msg);
            return null;
         } finally {
            if ( reader != null )
               reader.Close();
         }
      }

   } // class MailEx

} // namespace NAnt.Contrib.Tasks
