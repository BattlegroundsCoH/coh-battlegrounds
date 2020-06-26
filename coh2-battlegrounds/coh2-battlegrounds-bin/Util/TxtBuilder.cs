using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Battlegrounds.Util {
    
    /// <summary>
    /// 
    /// </summary>
    public sealed class TxtBuilder {

        StringBuilder m_internalStringBuilder;
        int m_indentLvl;

        private string _IN => (m_indentLvl > 0) ? new string('\t', m_indentLvl) : string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public TxtBuilder() {
            m_internalStringBuilder = new StringBuilder();
            m_indentLvl = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indent"></param>
        public void SetIndent(int indent) => m_indentLvl = indent;

        /// <summary>
        /// 
        /// </summary>
        public void IncreaseIndent() => m_indentLvl++;

        /// <summary>
        /// 
        /// </summary>
        public void DecreaseIndent() => m_indentLvl--;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        public void AppendLine(object content) => this.m_internalStringBuilder.Append($"{_IN}{content}\n");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        public void Append(object content) => this.m_internalStringBuilder.Append($"{_IN}{content}");

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetContent() => m_internalStringBuilder.ToString();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public void Save(string path) => File.WriteAllText(path, this.GetContent());

    }

}
