namespace Food.Models
{ /// <summary>
  /// 导入操作结果
  /// </summary>
    public class ImportResult
    {
        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结果消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 导入的记录数
        /// </summary>
        public int Count { get; set; }
    }
}
