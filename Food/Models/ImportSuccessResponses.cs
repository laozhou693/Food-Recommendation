namespace Food.Models
{
    /// <summary>
    /// 导入成功响应
    /// </summary>
    public class ImportSuccessResponse
    {
        /// <summary>
        /// 成功消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 导入记录数量
        /// </summary>
        public int Count { get; set; }
    }


}
