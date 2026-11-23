# Evergrove — hồ sơ chiến lược và bàn giao kỹ thuật (Tiếng Việt)

**Cập nhật:** 22/11/2026. **Trạng thái:** đã có mã nguồn Unity/C# và kiểm thử logic; **chưa có bằng chứng game đã chạy tốt trên iPhone/Android thật, chưa phát hành, chưa chứng minh product–market fit**. “Evergrove” là tên nội bộ, chưa được xác nhận quyền sử dụng thương mại.

**Hai nhóm tài liệu khác nhau:** Bộ [handbook hiện trạng](README.md) giải thích code đang chạy và giới hạn thật; [bộ đặc tả 37 tính năng đề xuất](specs/README.md) là backlog P0/P1/P2 **chưa có nghĩa được triển khai**. Không trộn “đang có” và “muốn xây”.


## 1. Ta thực sự đang xây thứ gì?

**Lời hứa:** “Một thế giới nhỏ thay đổi vì những điều người chơi đã làm.”

Mỗi vòng chơi dự kiến ngắn, nhẹ nhàng:
1. Xoay các đoạn ống để nối nước từ **nguồn** (biểu tượng sóng nước xanh) tới **đầu ra của vườn** (biểu tượng chồi xanh), không hở mối nối.
2. Nước được khôi phục, người chơi chọn **trồng cây**, **sửa cầu** hoặc **gieo hoa**.
3. Cảnh làng và Mira/Orin/Fen phản ánh đúng lựa chọn trong lịch sử.
4. **Màn sau bị ảnh hưởng về cơ chế:** lựa chọn trước xoay vị trí mạng đường nước, khóa một đoạn ống đại diện cho rễ cây / móng cầu / khu hoa cần bảo vệ.
5. Sau **12 chương**, lịch sử dẫn đến một số kết thúc được viết sẵn.

Điểm khác biệt cần kiểm nghiệm không phải “puzzle + trang trí”. Đó là **quyết định hôm nay tạo ràng buộc cho câu đố ngày mai**, đi cùng cảm giác nơi chốn và ký ức.

## 2. Điều nào đã làm được, điều nào chưa?

| Đã có mã nguồn | Chưa được chứng minh / chưa có |
| --- | --- |
| 12 màn gốc: 6 màn 3×3 và 6 màn 4×4 | Game chạy tốt trên thiết bị thật |
| 33 **biến thể tiếp theo** theo cây/cầu/hoa (11 chương × 3) | 33 màn độc lập hoàn toàn: hiện chỉ biến đổi bằng phép xoay mạng gốc và một ô cố định |
| Trạng thái, undo, hint, restart, khôi phục dữ liệu v1/v2/v3→v4 | Mô phỏng sinh thái thật sự, cây cần nước/ánh sáng, nền kinh tế, chuỗi nhân quả sâu |
| Cảnh làng 2D và thử nghiệm diorama 2.5D | Minh họa 2.5D đạt chất lượng mỹ thuật tương đương game dẫn đầu |
| 3 NPC có lời thoại phụ thuộc lựa chọn | NPC có ý chí, học hỏi, trí nhớ hội thoại hay AI runtime |
| 30 lựa chọn ngôn ngữ, 81 khóa mỗi bản | Bản dịch được người bản ngữ duyệt, RTL/Indic hiển thị chuẩn |
| Kiểm thử .NET và workflow build Unity thủ công | D1/D7, lượng tìm kiếm ASO, sẵn lòng trả tiền, kênh phân phối thực tế |

**Mấu chốt về tính trung thực:** 33 biến thể chỉ là cùng các đồ thị được xoay 90°/180°/270° và giữ cố định một nút. Đây là bước **cơ chế có hệ quả giới hạn**, chưa phải thế giới mô phỏng “sống” thật.

## 3. Kiến trúc nào phải giữ?

- **Core thuần C#:** `PipeRules` (đường nước), `ReactiveWatercourses` (biến thể), `WorldModel` (trạng thái thật), `VillageStory` (nội dung), `VillageScenePlan` (quyết định hình ảnh).
- **Unity chỉ hiển thị và lấy tương tác:** `EvergroveApp`, `VillageIllustration`, `VillageDiorama`, `PipeGraphic`.
- **Save offline:** `WorldStore` ghi file JSON có backup; schema v4, tên file hiện còn `evergrove-world-v2.json` để tương thích bản cũ.
- **Ngôn ngữ:** `GameLocalization` + `LocalizedVillage`; đổi ngôn ngữ không được đổi trạng thái game.
- **Cloudflare backend:** **chỉ là demo tách rời**, chưa đồng bộ lịch sử game, không nên bật mặc định.

Không dùng LLM để tạo ngẫu nhiên “ký ức” không có thật. Mọi lời thoại phải suy ra từ lịch sử lựa chọn kiểm chứng được.

## 4. Điểm mù lớn nhất

1. **Tưởng có simulation sâu trong khi mới có biến đổi puzzle đơn giản.** Cần một hệ nhân quả vật lý thật (ví dụ trồng cây thay đổi nguồn nước dành cho một khu khác) *chỉ sau khi* người chơi chứng minh họ quan tâm.
2. **So sánh A/B thiếu công bằng.** Từ chương 2, Living World có màn thay đổi còn Puzzle Only dùng màn gốc. Vì vậy không thể nói retention cao hơn là “nhờ NPC/câu chuyện”. Phải thử riêng chuyện, luật puzzle và mỹ thuật.
3. **“Game số một” là kỳ vọng chứ không phải mục tiêu kỹ thuật đủ rõ.** Chỉ quan sát người thật mới cho biết họ có muốn quay lại hay không.
4. **Phân phối và tên thương hiệu chưa rõ.** Kiểm tra thị trường/tên gọi trước khi đổ tiền vào UA và tài sản thương hiệu.
5. **30 ngôn ngữ chưa đồng nghĩa với 30 thị trường sẵn sàng.** RTL, font, bản dịch người bản ngữ và trải nghiệm người 45+ vẫn cần QA.

## 5. Một tuần tới nên làm gì?

**Ưu tiên duy nhất:** có một bản Unity thật, đưa cho người chơi thật, xem họ hiểu/quan tâm đến hệ quả như thế nào.

- **Ngày 1–2:** chạy Unity Editor, build Android/WebGL/iOS-Xcode, thử điện thoại; sửa lỗi touch, save và font.
- **Ngày 3:** tuyển 20–30 người phù hợp qua founder-led outreach, không chỉ người quen.
- **Ngày 4–5:** quan sát không hướng dẫn; hỏi “Vì sao màn sau khác?”; ghi lại cả người thất bại, bỏ cuộc, không thích.
- **Ngày 6:** so sánh 2D/2.5D với cùng dữ liệu và đối chứng có độ hoàn thiện tốt; đo D1 khi có đồng ý theo dõi.
- **Ngày 7:** ra **một quyết định: phát triển tiếp / đơn giản hóa / dừng**, dựa trên hành vi, không dựa vào lời khen.

**Điều kiện tiếp tục (đề xuất):** đa số người chơi tự hiểu cách chơi và nhận ra hệ quả; một tỷ lệ đáng kể tự chọn chơi màn tiếp theo; không mất dữ liệu và có kênh tiếp cận người chơi ngoài mạng lưới founder. **Nếu không:** sửa vòng lặp, không tăng thêm AI, cấp độ hay ngôn ngữ.

## 6. Nguồn triển khai chuẩn

- [Tổng quan và mục lục](README.md)
- [Mô tả sản phẩm](PRODUCT.md)
- [Luật gameplay chính xác](GAMEPLAY_AND_WORLD_RULES.md)
- [Kiến trúc](ARCHITECTURE.md)
- [Save/migration](SAVE_AND_MIGRATION.md)
- [Trải nghiệm, accessibility và bản địa hóa](PLAYER_EXPERIENCE.md)
- [Thiết kế thử nghiệm / kênh phân phối](VALIDATION.md)
- [Cách build và tiêu chí QA](BUILD_AND_QA.md)
- [Quyết định, rủi ro, phản biện](DECISIONS_AND_RISKS.md)

**Nguyên tắc cuối:** người chơi phải muốn quay về ngôi làng của mình. Nếu ta chỉ làm thêm tài liệu và code mà chưa nhìn thấy ai thực sự chơi, đó chưa phải tiến bộ sản phẩm.
