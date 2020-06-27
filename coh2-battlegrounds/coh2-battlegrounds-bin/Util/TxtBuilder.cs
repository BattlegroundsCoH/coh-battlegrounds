using System.IO;
using System.Text;

namespace Battlegrounds.Util {
    
    /// <summary>
    /// Simple text builder with support for indentation and lines. Content can be retrieved as a <see cref="string"/> or saved to a file. This class cannot be inherited.
    /// </summary>
    public sealed class TxtBuilder {

        StringBuilder m_internalStringBuilder;
        int m_indentLvl;

        private string IN => (m_indentLvl > 0) ? new string('\t', m_indentLvl) : string.Empty;

        /// <summary>
        /// Create an empty instance of a <see cref="TxtBuilder"/> with no indentation.
        /// </summary>
        public TxtBuilder() {
            m_internalStringBuilder = new StringBuilder();
            m_indentLvl = 0;
        }

        /// <summary>
        /// Set the indent level.
        /// </summary>
        /// <param name="indent">The indent level to set</param>
        public void SetIndent(int indent) => m_indentLvl = indent;

        /// <summary>
        /// Increase the indent level by one
        /// </summary>
        public void IncreaseIndent() => m_indentLvl++;

        /// <summary>
        /// Decrease the indent level by one
        /// </summary>
        public void DecreaseIndent() => m_indentLvl--;

        /// <summary>
        /// Add a new line to the text with proper indentation.
        /// </summary>
        /// <param name="content">The <see cref="object"/> content to append.</param>
        public void AppendLine(object content) => this.m_internalStringBuilder.Append($"{IN}{content}\n");

        /// <summary>
        /// Add content to the <see cref="TxtBuilder"/> with indentation.
        /// </summary>
        /// <param name="content">The <see cref="object"/> content to append.</param>
        public void Append(object content) => this.Append(content, true);

        /// <summary>
        /// Add content to the <see cref="TxtBuilder"/> where indentation use is specified.
        /// </summary>
        /// <param name="content">The <see cref="object"/> content to append.</param>
        /// <param name="indent">Use indentation when appending</param>
        public void Append(object content, bool indent) => this.m_internalStringBuilder.Append($"{((indent) ? (this.IN) : (string.Empty))}{content}");

        /// <summary>
        /// Retrieve the <see cref="string"/> contents of a <see cref="TxtBuilder"/>.
        /// </summary>
        /// <returns>The internal string contents built with the internal <see cref="StringBuilder"/>.</returns>
        public string GetContent() => m_internalStringBuilder.ToString();

        /// <summary>
        /// Get the internal <see cref="StringBuilder"/> used by the <see cref="TxtBuilder"/> instance to build its content.
        /// </summary>
        /// <returns>The <see cref="StringBuilder"/> instance used internally by the <see cref="TxtBuilder"/> instance.</returns>
        public StringBuilder GetStringBuilder() => m_internalStringBuilder;

        /// <summary>
        /// Save all the contents of a <see cref="TxtBuilder"/> to a specified file.
        /// </summary>
        /// <param name="path">The path of the file to write contents to.</param>
        public void Save(string path) => File.WriteAllText(path, this.GetContent());

    }

}
