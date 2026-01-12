# HIS Appointment Booking API Documentation

## Base URL
```
http://localhost:7239/api/AppointmentsApi
```

## Endpoints

### 1. L?y danh sách Khoa/Phòng Ban
**GET** `/Departments`

**Response:**
```json
{
  "success": true,
  "departments": [
    {
      "departmentId": 1,
      "code": "DEPT001",
      "name": "Khoa Noi"
    }
  ]
}
```

### 2. L?y danh sách Bác s?
**GET** `/Doctors?departmentId=1`

**Query Parameters:**
- `departmentId` (optional): L?c theo khoa

**Response:**
```json
{
  "success": true,
  "doctors": [
    {
      "staffId": 1,
      "fullName": "Dr. Nguyen Van A",
      "departmentId": 1,
      "departmentName": "Khoa Noi"
    }
  ]
}
```

### 3. L?y khung gi? còn tr?ng
**GET** `/AvailableSlots?date=2026-01-09&departmentId=1`

**Query Parameters:**
- `date` (required): Ngày khám (yyyy-MM-dd)
- `departmentId` (optional): L?c theo khoa

**Response:**
```json
{
  "success": true,
  "date": "2026-01-09",
  "slots": [
    {
      "timeSlotId": 1,
      "code": "S1",
      "start": "08:00",
      "end": "09:00",
      "booked": 3,
      "maxCapacity": 10,
      "available": 7
    }
  ]
}
```

### 4. ??t l?ch h?n
**POST** `/Book`

**Request Body:**
```json
{
  "fullName": "Nguyen Van B",
  "phone": "0123456789",
  "dob": "1990-01-01",
  "gender": 1,
  "departmentId": 1,
  "doctorId": 1,
  "date": "2026-01-09",
  "timeSlotId": 1,
  "note": "Kham suc khoe dinh ky"
}
```

**Gender Values:**
- 0: Unknown
- 1: Male (Nam)
- 2: Female (N?)
- 3: Other

**Response:**
```json
{
  "success": true,
  "message": "Dat lich thanh cong",
  "appointmentCode": "APT20260109123456",
  "appointmentId": 123,
  "date": "09/01/2026"
}
```

### 5. Ki?m tra l?ch h?n
**GET** `/Check?code=APT20260109123456`

**Query Parameters:**
- `code` (required): Mã l?ch h?n

**Response:**
```json
{
  "success": true,
  "appointment": {
    "code": "APT20260109123456",
    "status": "Booked",
    "date": "09/01/2026",
    "timeSlot": "08:00 - 09:00",
    "patient": {
      "fullName": "Nguyen Van B",
      "phone": "0123456789",
      "gender": "Male"
    },
    "department": "Khoa Noi",
    "doctor": "Dr. Nguyen Van A",
    "note": "Kham suc khoe dinh ky"
  }
}
```

## Error Responses

**400 Bad Request:**
```json
{
  "success": false,
  "message": "Vui long nhap day du thong tin"
}
```

**404 Not Found:**
```json
{
  "success": false,
  "message": "Khong tim thay lich hen"
}
```

**500 Internal Server Error:**
```json
{
  "success": false,
  "message": "Loi server"
}
```

## CORS Configuration

API h? tr? CORS cho domain ???c c?u hình trong `appsettings.json`:
```json
{
  "Portal": {
    "AllowedOrigin": "http://localhost:3000"
  }
}
```

## Complete Flow Example

### B??c 1: L?y danh sách khoa
```javascript
fetch('http://localhost:7239/api/AppointmentsApi/Departments')
  .then(res => res.json())
  .then(data => console.log(data.departments));
```

### B??c 2: L?y danh sách bác s? theo khoa
```javascript
const departmentId = 1;
fetch(`http://localhost:7239/api/AppointmentsApi/Doctors?departmentId=${departmentId}`)
  .then(res => res.json())
  .then(data => console.log(data.doctors));
```

### B??c 3: L?y khung gi? còn tr?ng
```javascript
const date = '2026-01-09';
fetch(`http://localhost:7239/api/AppointmentsApi/AvailableSlots?date=${date}`)
  .then(res => res.json())
  .then(data => console.log(data.slots));
```

### B??c 4: ??t l?ch
```javascript
const bookingData = {
  fullName: "Nguyen Van B",
  phone: "0123456789",
  dob: "1990-01-01",
  gender: 1,
  departmentId: 1,
  doctorId: 1,
  date: "2026-01-09",
  timeSlotId: 1,
  note: "Kham suc khoe"
};

fetch('http://localhost:7239/api/AppointmentsApi/Book', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  },
  body: JSON.stringify(bookingData)
})
.then(res => res.json())
.then(data => {
  console.log('Appointment Code:', data.appointmentCode);
});
```

### B??c 5: Ki?m tra l?ch ?ã ??t
```javascript
const code = 'APT20260109123456';
fetch(`http://localhost:7239/api/AppointmentsApi/Check?code=${code}`)
  .then(res => res.json())
  .then(data => console.log(data.appointment));
```

## Notes

- T?t c? ngày tháng s? d?ng format ISO 8601 (yyyy-MM-dd)
- S? ?i?n tho?i là duy nh?t cho m?i b?nh nhân
- M?i khung gi? gi?i h?n t?i ?a 10 l?ch h?n
- L?ch h?n có 3 tr?ng thái: Booked, CheckedIn, Cancelled
