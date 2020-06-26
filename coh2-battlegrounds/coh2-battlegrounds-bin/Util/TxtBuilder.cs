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

        /// <summary>
        /// 
        /// </summary>
        public TxtBuilder() {
            m_internalStringBuilder = new StringBuilder();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        public void AppendLine(object content) => this.m_internalStringBuilder.Append($"{content}\n");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        public void Append(object content) => this.m_internalStringBuilder.Append(content);

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
