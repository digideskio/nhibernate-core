using System;
using StringHelper = NHibernate.Util.StringHelper;
using Element = System.Xml.XmlElement;

namespace NHibernate.Tool.hbm2net
{
	
	
	/// <summary> </summary>
	public class Generator
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private void  InitBlock()
		{
			suffix = string.Empty;
			prefix = string.Empty;
		}
		virtual public System.String BaseDirName
		{
			get
			{
				return baseDirName;
			}
			
			set
			{
				if ((System.Object) value != null)
				{
					this.baseDirName = value;
				}
			}
			
		}
		
		private System.String rendererClass = "NHibernate.Tool.hbm2net.BasicRenderer";
		private System.String baseDirName = "generated";
		private System.String packageName = null;
		private System.String suffix;
		private System.String prefix;
		private System.String extension = "cs";
		private bool lowerFirstLetter = false;
		
		public static System.Collections.Specialized.NameValueCollection params_Renamed;
		
		/// <summary> Constructs a new Generator using the defaults.</summary>
		public Generator()
		{
			InitBlock();
		}
		
		/// <summary> Constructs a new Generator, configured from XML.</summary>
		public Generator(Element generateElement)
		{
			InitBlock();
			System.String value_Renamed = null;
			
			// set rendererClass field
			if ((System.Object) (this.rendererClass = (generateElement.Attributes["renderer"] == null?null:generateElement.Attributes["renderer"].Value)) == null)
			{
				throw new System.Exception("attribute renderer is required.");
			}
			
			// set dirName field
			if ((System.Object) (value_Renamed = (generateElement.Attributes["dir"] == null?null:generateElement.Attributes["dir"].Value)) != null)
			{
				this.baseDirName = value_Renamed;
			}
			
			// set packageName field
			this.packageName = (generateElement.Attributes["package"] == null?null:generateElement.Attributes["package"].Value);
			
			// set prefix
			if ((System.Object) (value_Renamed = (generateElement.Attributes["prefix"] == null?null:generateElement.Attributes["prefix"].Value)) != null)
			{
				this.prefix = value_Renamed;
			}
			
			// set suffix
			if ((System.Object) (value_Renamed = (generateElement.Attributes["suffix"] == null?null:generateElement.Attributes["suffix"].Value)) != null)
			{
				this.suffix = value_Renamed;
			}
			
			// set extension
			if ((System.Object) (value_Renamed = (generateElement.Attributes["extension"] == null?null:generateElement.Attributes["extension"].Value)) != null)
			{
				this.extension = value_Renamed;
			}
			
			// set lowerFirstLetter
			value_Renamed = (generateElement.Attributes["lowerFirstLetter"] == null?null:generateElement.Attributes["lowerFirstLetter"].Value);
			try
			{
				this.lowerFirstLetter = System.Boolean.Parse(value_Renamed);
			}
			catch{}
			
			params_Renamed = new System.Collections.Specialized.NameValueCollection();
			System.Collections.IEnumerator iter = generateElement.GetElementsByTagName("param").GetEnumerator();
			while (iter.MoveNext())
			{
				Element childNode = (Element) iter.Current;
				params_Renamed[childNode.Attributes["name"].Value] = childNode.InnerText;
			}
		}
		
		/// <summary> </summary>
		public virtual void  generate(System.Collections.IDictionary classMappingsCol)
		{
			log.Info("Generating " + classMappingsCol.Count + " in " + BaseDirName);
			Renderer renderer = (Renderer) SupportClass.CreateNewInstance(System.Type.GetType(this.rendererClass));
			
			/// <summary>Configure renderer </summary>
			renderer.configure(params_Renamed);
			
			/// <summary>Running through actual classes </summary>
			for (System.Collections.IEnumerator classMappings = classMappingsCol.Values.GetEnumerator(); classMappings.MoveNext(); )
			{
				ClassMapping classMapping = (ClassMapping) classMappings.Current;
				writeRecur(classMapping, classMappingsCol, renderer);
			}
			/// <summary>Running through components </summary>
			for (System.Collections.IEnumerator cmpMappings = ClassMapping.Components; cmpMappings.MoveNext(); )
			{
				ClassMapping mapping = (ClassMapping) cmpMappings.Current;
				write(mapping, classMappingsCol, renderer);
			}
		}
		
		private void  writeRecur(ClassMapping classMapping, System.Collections.IDictionary class2classmap, Renderer renderer)
		{
			
			write(classMapping, class2classmap, renderer);
			
			if (!(classMapping.Subclasses.Count == 0))
			{
				System.Collections.IEnumerator it = classMapping.Subclasses.GetEnumerator();
				while (it.MoveNext())
				{
					writeRecur((ClassMapping) it.Current, class2classmap, renderer);
				}
			}
		}
		
		
		/// <summary> </summary>
		private void  write(ClassMapping classMapping, System.Collections.IDictionary class2classmap, Renderer renderer)
		{
			System.String saveToPackage = renderer.getSaveToPackage(classMapping);
			System.String saveToClassName = renderer.getSaveToClassName(classMapping);
			System.IO.FileInfo dir = this.getDir(saveToPackage);
			System.IO.FileInfo file = new System.IO.FileInfo(dir.FullName + "\\" + this.getFileName(saveToClassName));
			log.Debug("Writing " + file);
			
			System.IO.StreamWriter writer = new System.IO.StreamWriter(new System.IO.FileStream(file.FullName, System.IO.FileMode.Create));
			
			renderer.render(getPackageName(saveToPackage), getName(saveToClassName), classMapping, class2classmap, writer);
			writer.Close();
		}
		
		/// <summary> </summary>
		private System.String getFileName(System.String className)
		{
			return this.getName(className) + "." + this.extension;
		}
		
		/// <summary> </summary>
		private System.String getName(System.String className)
		{
			System.String name = null;
			
			if (this.lowerFirstLetter)
			{
				name = className.Substring(0, (1) - (0)).ToLower() + className.Substring(1, (className.Length) - (1));
			}
			else
			{
				name = className;
			}
			
			return this.prefix + name + this.suffix;
		}
		
		private System.String getPackageName(System.String packageName)
		{
			if ((System.Object) this.packageName == null)
			{
				return (System.Object) packageName == null?string.Empty:packageName;
			}
			else
			{
				return this.packageName;
			}
		}
		/// <summary> </summary>
		private System.IO.FileInfo getDir(System.String packageName)
		{
			System.IO.FileInfo baseDir = new System.IO.FileInfo(this.baseDirName);
			System.IO.FileInfo dir = null;
			
			System.String p = getPackageName(packageName);
			
			dir = new System.IO.FileInfo(baseDir.FullName + "\\" + p.Replace(StringHelper.Dot, System.IO.Path.DirectorySeparatorChar));
			
			// if the directory exists, make sure it is a directory
			bool tmpBool;
			if (System.IO.File.Exists(dir.FullName))
				tmpBool = true;
			else
				tmpBool = System.IO.Directory.Exists(dir.FullName);
			if (tmpBool)
			{
				if (!System.IO.Directory.Exists(dir.FullName))
				{
					throw new System.Exception("The path: " + dir.FullName + " exists, but is not a directory");
				}
			}
			// else make the directory and any non-existent parent directories
			else
			{
				if (!System.IO.Directory.CreateDirectory(dir.FullName).Exists)
				{
					throw new System.Exception("unable to create directory: " + dir.FullName);
				}
			}
			
			return dir;
		}
	}
}