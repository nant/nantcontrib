//
// NAntContrib - NAntAddin
// Copyright (C) 2002 Jayme C. Edwards (jedwards@wi.rr.com)
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
using System.Xml;
using System.ComponentModel;
using System.Windows.Forms;

namespace NAnt.Contrib.NAntAddin.Nodes
{
	/// <summary>
	/// Tree Node that represents an NAnt mail task.
	/// </summary>
	/// <remarks>None.</remarks>
	[NAntTask("mail", "Send an E-Mail Message", "mailtask.bmp")]
	public class NAntSendSMTPMailTaskNode : NAntTaskNode
	{
		/// <summary>
		/// Creates a new <see cref="NAntSendSMTPMailTaskNode"/>.
		/// </summary>
		/// <param name="TaskElement">The task's XML element.</param>
		/// <param name="ParentElement">The parent XML element of the task.</param>
		/// <remarks>None.</remarks>
		public NAntSendSMTPMailTaskNode(XmlElement TaskElement, XmlElement ParentElement) 
			: base(TaskElement, ParentElement)
		{
		}

		/// <summary>
		/// Gets or sets the email address of the sender.
		/// </summary>
		/// <value>The email address of the sender.</value>
		/// <remarks>None.</remarks>
		[Description("Email address of the sender."),Category("Data")]
		public string From
		{
			get
			{
				return TaskElement.GetAttribute("from");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("from");
				}
				else
				{
					TaskElement.SetAttribute("from", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets comma or semicolon separated list of recipient email addresses.
		/// </summary>
		/// <value>Comma or semicolon separated list of recipient email addresses.</value>
		/// <remarks>None.</remarks>
		[Description("Comma or semicolon separated list of recipient email addresses."),Category("Data")]
		public string ToList
		{
			get
			{
				return TaskElement.GetAttribute("tolist");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("tolist");
				}
				else
				{
					TaskElement.SetAttribute("tolist", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets comma or semicolon separated list of copied email addresses.
		/// </summary>
		/// <value>Comma or semicolon separated list of copied email addresses.</value>
		/// <remarks>None.</remarks>
		[Description("Comma or semicolon separated list of copied email addresses."),Category("Data")]
		public string CCList
		{
			get
			{
				return TaskElement.GetAttribute("cclist");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("clist");
				}
				else
				{
					TaskElement.SetAttribute("clist", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets comma or semicolon separated list of blind copied email addresses.
		/// </summary>
		/// <value>Comma or semicolon separated list of blind copied email addresses.</value>
		/// <remarks>None.</remarks>
		[Description("Comma or semicolon separated list of blind copied email addresses."),Category("Data")]
		public string BCCList
		{
			get
			{
				return TaskElement.GetAttribute("bcclist");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("bcclist");
				}
				else
				{
					TaskElement.SetAttribute("bcclist", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the host name of mail server. 
		/// </summary>
		/// <value>The host name of mail server. </value>
		/// <remarks>None.</remarks>
		[Description("Host name of mail server. Defaults to \"localhost\"."),Category("Data")]
		public string MailHost
		{
			get
			{
				return TaskElement.GetAttribute("mailhost");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("mailhost");
				}
				else
				{
					TaskElement.SetAttribute("mailhost", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the text body of the email message.
		/// </summary>
		/// <value>The text body of the email message.</value>
		/// <remarks>None.</remarks>
		[Description("Text body of the email message."),Category("Data")]
		public string Message
		{
			get
			{
				return TaskElement.GetAttribute("message");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("message");
				}
				else
				{
					TaskElement.SetAttribute("message", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the subject of the email message.
		/// </summary>
		/// <value>The subject of the email message.</value>
		/// <remarks>None.</remarks>
		[Description("Subject of the email message."),Category("Data")]
		public string Subject
		{
			get
			{
				return TaskElement.GetAttribute("subject");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("subject");
				}
				else
				{
					TaskElement.SetAttribute("subject", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the format of the message body.
		/// </summary>
		/// <value>The format of the message body.</value>
		/// <remarks>None.</remarks>
		[Description("Format of the message body. Valid values are \"Html\" or \"Text\". Defaults to \"Text\"."),Category("Data")]
		public string Format
		{
			get
			{
				return TaskElement.GetAttribute("format");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("format");
				}
				else
				{
					TaskElement.SetAttribute("format", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the name(s) of text files to send as part of body of the email message.
		/// </summary>
		/// <value>The name(s) of text files to send as part of body of the email message.</value>
		/// <remarks>None.</remarks>
		[Description("Name(s) of text files to send as part of body of the email message. Multiple file names are comma or semicolon separated."),Category("Data")]
		public string Files
		{
			get
			{
				return TaskElement.GetAttribute("files");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("files");
				}
				else
				{
					TaskElement.SetAttribute("files", value);
				}
				Save();
			}
		}

		/// <summary>
		/// Gets or sets the name(s) of files to send as attachments to email message.
		/// </summary>
		/// <value>The name(s) of files to send as attachments to email message.</value>
		/// <remarks>None.</remarks>
		[Description("Name(s) of files to send as attachments to email message. Multiple file names are comma or semicolon separated."),Category("Data")]
		public string Attachments
		{
			get
			{
				return TaskElement.GetAttribute("attachments");
			}

			set
			{
				if (value == "")
				{
					TaskElement.RemoveAttribute("attachments");
				}
				else
				{
					TaskElement.SetAttribute("attachments", value);
				}
				Save();
			}
		}
	}
}