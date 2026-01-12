# ?? HIS APPOINTMENT BOOKING API - H??NG D?N TÍCH H?P

## ?? T?NG QUAN

API này cho phép website bên ngoài tích h?p ch?c n?ng ??t l?ch khám b?nh v?i H? Th?ng Qu?n Lý B?nh Vi?n (HIS).

## ?? TÍNH N?NG

? Xem danh sách khoa/phòng ban  
? Xem danh sách bác s? theo khoa  
? Xem khung gi? còn tr?ng theo ngày  
? ??t l?ch khám b?nh  
? Tra c?u thông tin l?ch h?n  

---

## ?? QUICK START

### B??c 1: C?u hình CORS

Trong file `appsettings.json`, c?p nh?t domain c?a website b?n:

```json
{
  "Portal": {
    "AllowedOrigin": "http://your-website.com"
  }
}
```

N?u có nhi?u domain, s?a trong `Program.cs`:

```csharp
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("Portal", p =>
        p.WithOrigins("http://localhost:3000", "https://your-website.com")
         .AllowAnyHeader()
         .AllowAnyMethod());
});
```

### B??c 2: Test API

M? file HTML m?u trong trình duy?t:
```
http://localhost:7239/booking-example.html
```

### B??c 3: Tích h?p vào website c?a b?n

Xem file `API_DOCUMENTATION.md` ?? bi?t chi ti?t các endpoint.

---

## ?? CÁC ENDPOINT CHÍNH

### 1?? L?y danh sách Khoa
```javascript
GET /api/AppointmentsApi/Departments
```

### 2?? L?y danh sách Bác s?
```javascript
GET /api/AppointmentsApi/Doctors?departmentId=1
```

### 3?? L?y khung gi? tr?ng
```javascript
GET /api/AppointmentsApi/AvailableSlots?date=2026-01-09
```

### 4?? ??t l?ch
```javascript
POST /api/AppointmentsApi/Book
Content-Type: application/json

{
  "fullName": "Nguyen Van A",
  "phone": "0123456789",
  "departmentId": 1,
  "date": "2026-01-09",
  "timeSlotId": 1
}
```

### 5?? Tra c?u l?ch h?n
```javascript
GET /api/AppointmentsApi/Check?code=APT20260109123456
```

---

## ?? VÍ D? CODE

### React Example

```jsx
import { useState, useEffect } from 'react';

const API_BASE = 'http://localhost:7239/api/AppointmentsApi';

function BookingForm() {
  const [departments, setDepartments] = useState([]);
  const [doctors, setDoctors] = useState([]);
  const [slots, setSlots] = useState([]);

  useEffect(() => {
    // Load departments
    fetch(`${API_BASE}/Departments`)
      .then(res => res.json())
      .then(data => setDepartments(data.departments));
  }, []);

  const loadDoctors = (departmentId) => {
    fetch(`${API_BASE}/Doctors?departmentId=${departmentId}`)
      .then(res => res.json())
      .then(data => setDoctors(data.doctors));
  };

  const loadSlots = (date) => {
    fetch(`${API_BASE}/AvailableSlots?date=${date}`)
      .then(res => res.json())
      .then(data => setSlots(data.slots));
  };

  const bookAppointment = async (formData) => {
    const response = await fetch(`${API_BASE}/Book`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(formData)
    });
    const data = await response.json();
    
    if (data.success) {
      alert(`Dat lich thanh cong! Ma: ${data.appointmentCode}`);
    }
  };

  return (
    // Your form JSX here
  );
}
```

### Vue.js Example

```vue
<script setup>
import { ref, onMounted } from 'vue';

const API_BASE = 'http://localhost:7239/api/AppointmentsApi';
const departments = ref([]);
const doctors = ref([]);
const slots = ref([]);

onMounted(async () => {
  const res = await fetch(`${API_BASE}/Departments`);
  const data = await res.json();
  departments.value = data.departments;
});

const loadDoctors = async (departmentId) => {
  const res = await fetch(`${API_BASE}/Doctors?departmentId=${departmentId}`);
  const data = await res.json();
  doctors.value = data.doctors;
};

const bookAppointment = async (formData) => {
  const res = await fetch(`${API_BASE}/Book`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(formData)
  });
  const data = await res.json();
  return data;
};
</script>
```

### jQuery Example

```javascript
const API_BASE = 'http://localhost:7239/api/AppointmentsApi';

// Load departments
$.get(`${API_BASE}/Departments`, function(data) {
  data.departments.forEach(dept => {
    $('#department').append(
      `<option value="${dept.departmentId}">${dept.name}</option>`
    );
  });
});

// Book appointment
$('#bookingForm').submit(function(e) {
  e.preventDefault();
  
  const formData = {
    fullName: $('#fullName').val(),
    phone: $('#phone').val(),
    departmentId: parseInt($('#department').val()),
    date: $('#date').val(),
    timeSlotId: parseInt($('#timeSlot').val())
  };
  
  $.ajax({
    url: `${API_BASE}/Book`,
    type: 'POST',
    contentType: 'application/json',
    data: JSON.stringify(formData),
    success: function(data) {
      if (data.success) {
        alert('Dat lich thanh cong! Ma: ' + data.appointmentCode);
      }
    }
  });
});
```

---

## ?? B?O M?T

### API Key Authentication (Khuy?n ngh?)

?? b?o v? API, b?n nên thêm API Key authentication:

```csharp
// Them vao AppointmentsApiController.cs
[ApiController]
[Route("api/[controller]")]
public class AppointmentsApiController : ControllerBase
{
    [ServiceFilter(typeof(ApiKeyAuthFilter))]
    public async Task<IActionResult> Book([FromBody] BookAppointmentRequest request)
    {
        // ...
    }
}
```

T?o filter:

```csharp
public class ApiKeyAuthFilter : IActionFilter
{
    private const string ApiKeyHeaderName = "X-API-Key";
    
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKey))
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        
        var validApiKey = "YOUR_SECRET_API_KEY"; // Luu trong config
        if (apiKey != validApiKey)
        {
            context.Result = new UnauthorizedResult();
        }
    }
    
    public void OnActionExecuted(ActionExecutedContext context) { }
}
```

S? d?ng t? client:

```javascript
fetch(`${API_BASE}/Book`, {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'X-API-Key': 'YOUR_SECRET_API_KEY'
  },
  body: JSON.stringify(formData)
});
```

---

## ?? TESTING

### Test v?i Postman

1. Import collection t? file `API_DOCUMENTATION.md`
2. Set base URL: `http://localhost:7239/api/AppointmentsApi`
3. Test t?ng endpoint

### Test v?i cURL

```bash
# Get departments
curl http://localhost:7239/api/AppointmentsApi/Departments

# Get available slots
curl "http://localhost:7239/api/AppointmentsApi/AvailableSlots?date=2026-01-09"

# Book appointment
curl -X POST http://localhost:7239/api/AppointmentsApi/Book \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "Test User",
    "phone": "0123456789",
    "departmentId": 1,
    "date": "2026-01-09",
    "timeSlotId": 1
  }'
```

---

## ?? UI/UX TIPS

### 1. Hi?n th? Loading State
```javascript
<div v-if="loading">Dang tai...</div>
```

### 2. Validation Form
```javascript
if (!phone.match(/^[0-9]{10}$/)) {
  alert('So dien thoai khong hop le!');
  return;
}
```

### 3. Confirmation Dialog
```javascript
if (confirm('Xac nhan dat lich?')) {
  bookAppointment();
}
```

### 4. Success Message
```javascript
Swal.fire({
  icon: 'success',
  title: 'Dat lich thanh cong!',
  text: `Ma lich hen: ${appointmentCode}`
});
```

---

## ?? RESPONSIVE DESIGN

API tr? v? JSON thu?n túy, phù h?p cho:
- ?? Desktop websites
- ?? Mobile apps (iOS/Android)
- ?? Progressive Web Apps (PWA)
- ?? Mobile-responsive websites

---

## ?? L?U Ý

1. **Rate Limiting:** Nên thêm gi?i h?n s? l?n g?i API
2. **Caching:** Cache danh sách khoa, bác s? ?? gi?m t?i
3. **Validation:** Validate input ? c? client và server
4. **Error Handling:** X? lý l?i network, timeout
5. **Timezone:** API s? d?ng UTC, c?n convert sang local time

---

## ?? TROUBLESHOOTING

### L?i CORS
```
Access-Control-Allow-Origin error
```
**Gi?i pháp:** Ki?m tra c?u hình CORS trong `Program.cs` và `appsettings.json`

### L?i 500 Internal Server Error
**Gi?i pháp:** Ki?m tra logs trong Visual Studio Output window

### Khung gi? không hi?n th?
**Gi?i pháp:** ??m b?o database có d? li?u TimeSlots

---

## ?? H? TR?

- ?? Email: support@his.local
- ?? Documentation: `API_DOCUMENTATION.md`
- ?? Demo: `http://localhost:7239/booking-example.html`

---

## ?? LICENSE

MIT License - T? do s? d?ng cho m?c ?ích th??ng m?i và phi th??ng m?i.
