using System;
using System.Collections.Generic;
using UnityEngine;

namespace DraftUtils.Utils
{
    public static class Common
    {
        /// <summary>
        /// Gán dữ liệu cho danh sách item theo từng cặp tương ứng (item[i] ↔ dataList[i]).
        /// Nếu số lượng item nhiều hơn số lượng data, các item dư ra sẽ được xử lý bằng
        /// callback riêng (ví dụ: ẩn đi hoặc hiển thị trạng thái rỗng).
        /// </summary>
        /// <typeparam name="TItem">Kiểu của đối tượng item (ví dụ: UI item, cell, row...).</typeparam>
        /// <typeparam name="TData">Kiểu dữ liệu được gán cho mỗi item.</typeparam>
        /// <param name="itemList">Danh sách các item cần được gán dữ liệu hoặc xử lý khi thiếu dữ liệu.</param>
        /// <param name="dataList">Danh sách dữ liệu tương ứng, được ghép theo chỉ số (index) với itemList.</param>
        /// <param name="onItemHasData">Callback được gọi cho mỗi item có data tương ứng, nhận vào (item, data).</param>
        /// <param name="onItemNoData">Callback được gọi cho mỗi item dư ra không có data tương ứng (khi itemList dài hơn dataList).</param>
        public static void SetItems<TItem, TData>(
            List<TItem> itemList,
            List<TData> dataList,
            Action<TItem, TData> onItemHasData,
            Action<TItem> onItemNoData)
        {
            // Số cặp item-data có thể ghép được, giới hạn bởi danh sách ngắn hơn
            var pairedCount = Mathf.Min(itemList.Count, dataList.Count);

            // Gán data cho từng item theo đúng chỉ số tương ứng
            for (int i = 0; i < pairedCount; i++)
            {
                var item = itemList[i];
                var dataForItem = dataList[i];
                onItemHasData?.Invoke(item, dataForItem);
            }

            // Các item còn dư (nhiều hơn số lượng data hiện có) sẽ được xử lý riêng
            for (int i = pairedCount; i < itemList.Count; i++)
            {
                var item = itemList[i];
                onItemNoData?.Invoke(item);
            }
        }

        /// <summary>
        /// Rút gọn số thành chuỗi hiển thị K, M, B nếu giá trị lớn hơn hoặc bằng 10,000.
        /// </summary>
        /// <param name="value">Giá trị số cần chuyển đổi.</param>
        /// <returns>Chuỗi số đã định dạng (ví dụ: 15200 -> "15.2K", 2500000 -> "2.5M").</returns>
        public static string FormatNumber(double value)
        {
            if (value < 10000)
            {
                return value.ToString("0");
            }

            if (value >= 1000000000) // Tỷ (Billion)
            {
                return (value / 1000000000.0).ToString("0.#") + "B";
            }
            if (value >= 1000000) // Triệu (Million)
            {
                return (value / 1000000.0).ToString("0.#") + "M";
            }
            // Nghìn (Thousand)
            return (value / 1000.0).ToString("0.#") + "K";
        }
    }
    public class CameraInput
    {

        public static Vector3 GetMouseWorldPositionAtY(Vector3 inputMouse, float y)
        {
            var ray = Camera.main.ScreenPointToRay(inputMouse);
            var plane = new Plane(Vector3.up, new Vector3(0f, y, 0f));
            if (plane.Raycast(ray, out float enter))
            {
                return ray.GetPoint(enter);
            }

            return Vector3.zero;
        }


        /// <summary>
        /// Tính vị trí spawn theo danh sách neo (anchor).
        /// Nếu index vượt quá danh sách, ngoại suy tuyến tính từ 2 neo cuối.
        /// </summary>
        /// <param name="spawnIndex">Thứ tự vị trí cần lấy</param>
        /// <param name="anchors">Danh sách các điểm neo tham chiếu</param>
        /// <param name="getPositionFunc">Hàm lấy Vector3 từ một anchor</param>
        public static Vector3 GetSpawnPositionByIndex<T>(int spawnIndex, List<T> anchors, Func<T, Vector3> getPositionFunc)
            where T : Component
        {
            if (anchors.Count < 2)
            {
                throw new Exception("Cần ít nhất 2 anchor.");
            }
            if (getPositionFunc == null)
            {
                throw new Exception("Hàm getPosition không được null.");
            }

            // Nếu index nằm trong danh sách, trả về vị trí anchor tương ứng
            if (spawnIndex < anchors.Count)
            {
                return getPositionFunc(anchors[spawnIndex]);
            }

            // Ngoại suy: lấy 2 anchor cuối để xác định hướng và khoảng cách
            var secondLast = getPositionFunc(anchors[anchors.Count - 2]); // Anchor áp cuối
            var last = getPositionFunc(anchors[anchors.Count - 1]); // Anchor cuối

            var direction = (last - secondLast).normalized;  // Hướng từ áp cuối → cuối
            float spacing = Vector3.Distance(secondLast, last); // Khoảng cách giữa 2 anchor cuối
            int extraSteps = spawnIndex - anchors.Count + 1;  // Số bước cần đi thêm sau anchor cuối

            // Chiếu thẳng theo hướng đã xác định, mỗi bước = spacing
            return new Ray(last, direction).GetPoint(spacing * extraSteps);
        }


        public static List<Vector2Int> RotateZ(List<Vector2Int> parts, float angleZ)
        {
            List<Vector2Int> result = new List<Vector2Int>();

            float radians = angleZ * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);

            foreach (Vector2Int part in parts)
            {
                float rotatedX = part.x * cos - part.y * sin;
                float rotatedY = part.x * sin + part.y * cos;

                result.Add(new Vector2Int(Mathf.RoundToInt(rotatedX), Mathf.RoundToInt(rotatedY)));
            }

            return result;
        }
    }
}