using FastFoodBranch.Models;

namespace FastFoodBranch.Entities
{
    public class ConnectSingleton
    {
        
        private static QuanLyFastFoodEntities1 _instance;
        //Đề phòng trường hợp Connect đang được tạo ra từ nhiều phía *_lock
        private static readonly object _lock = new object();

        public static QuanLyFastFoodEntities1 GetInstance()
        {
            //Đảm bảo một luồng duy nhất được xử lí và trả về instance
            lock (_lock)
            {
                //Kiểm tra đã từng khởi tạo đối tượng hay chưa
                if (_instance == null)
                {
                    //Tạo đối tượng mới nếu chưa từng tạo
                    _instance = new QuanLyFastFoodEntities1();
                }
                //Trả về đối tượng nếu đã tạo trước đó
                return _instance;
            }
        }
    }
}
