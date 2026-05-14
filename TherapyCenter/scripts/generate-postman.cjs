const fs = require("fs");
const path = require("path");

const outDir = path.join(__dirname, "..");

const jsonHeader = [{ key: "Content-Type", value: "application/json", type: "text" }];

function bearer(tokenVar) {
  return {
    type: "bearer",
    bearer: [{ key: "token", value: tokenVar, type: "string" }],
  };
}

function req(name, opts) {
  const { method, urlRaw, description, bodyRaw, auth, tests } = opts;
  const r = {
    name,
    request: {
      method,
      header: [...jsonHeader],
      url: urlRaw,
      description: description || "",
    },
  };
  if (auth) r.request.auth = auth;
  if (bodyRaw)
    r.request.body = { mode: "raw", raw: bodyRaw, options: { raw: { language: "json" } } };
  if (tests && tests.length) {
    r.event = [
      {
        listen: "test",
        script: { exec: tests, type: "text/javascript" },
      },
    ];
  }
  return r;
}

const loginTests = [
  "pm.test('Status is 200', function () { pm.response.to.have.status(200); });",
  "try {",
  "  const j = pm.response.json();",
  "  if (j && j.token) {",
  "    pm.environment.set('token', j.token);",
  "    pm.environment.set('userId', String(j.userId));",
  "    pm.environment.set('userEmail', j.email || '');",
  "    pm.environment.set('userRole', j.role || '');",
  "    if (j.role === 'Admin') pm.environment.set('adminToken', j.token);",
  "    if (j.role === 'Guardian') pm.environment.set('guardianToken', j.token);",
  "    if (j.role === 'Receptionist') pm.environment.set('receptionistToken', j.token);",
  "    if (j.role === 'Doctor') pm.environment.set('doctorToken', j.token);",
  "  }",
  "} catch (e) { console.warn(e); }",
];

const captureTherapyIdTests = [
  "if (pm.response.code === 200) {",
  "  const j = pm.response.json();",
  "  if (j && j.therapyId != null) pm.environment.set('therapyId', String(j.therapyId));",
  "}",
];

const captureDoctorIdTests = [
  "if (pm.response.code === 200) {",
  "  const j = pm.response.json();",
  "  if (j && j.doctorId != null) pm.environment.set('doctorId', String(j.doctorId));",
  "}",
];

const captureSlotIdTests = [
  "if (pm.response.code === 200) {",
  "  const j = pm.response.json();",
  "  if (j && j.slotId != null) pm.environment.set('slotId', String(j.slotId));",
  "}",
];

const captureAppointmentIdTests = [
  "if (pm.response.code === 200) {",
  "  const j = pm.response.json();",
  "  if (j && j.appointmentId != null) pm.environment.set('appointmentId', String(j.appointmentId));",
  "}",
];

const captureFirstPatientIdTests = [
  "if (pm.response.code === 200) {",
  "  const arr = pm.response.json();",
  "  if (Array.isArray(arr) && arr.length && arr[0].patientId != null)",
  "    pm.environment.set('patientId', String(arr[0].patientId));",
  "}",
];

const captureRazorpayOrderIdTests = [
  "if (pm.response.code === 200) {",
  "  const j = pm.response.json();",
  "  if (j && j.orderId) pm.environment.set('razorpayOrderId', String(j.orderId));",
  "}",
];

const captureDoctorFindingIdTests = [
  "if (pm.response.code === 200) {",
  "  const j = pm.response.json();",
  "  if (j && j.findingId != null) pm.environment.set('doctorFindingId', String(j.findingId));",
  "}",
];

const registerGuardianBody = JSON.stringify(
  {
    firstName: "Priya",
    lastName: "Sharma",
    email: "guardian.priya@example.com",
    password: "Guardian@123",
    role: "Guardian",
    phoneNumber: "+919876543210",
  },
  null,
  2
);

const registerReceptionistBody = JSON.stringify(
  {
    firstName: "Ravi",
    lastName: "Kumar",
    email: "reception.ravi@example.com",
    password: "Reception@123",
    role: "Receptionist",
    phoneNumber: "+919876543211",
  },
  null,
  2
);

const loginAdminBody = JSON.stringify({ email: "admin@therapy.com", password: "123456" }, null, 2);
const loginGuardianBody = JSON.stringify({ email: "guardian.priya@example.com", password: "Guardian@123" }, null, 2);
const loginReceptionistBody = JSON.stringify({ email: "reception.ravi@example.com", password: "Reception@123" }, null, 2);

const createTherapyBody = JSON.stringify(
  {
    name: "Occupational Therapy — Fine Motor",
    description: "45-minute session focusing on hand strength and coordination.",
    durationMinutes: 45,
    cost: 1850.0,
  },
  null,
  2
);

const updateTherapyBody = JSON.stringify(
  {
    name: "Occupational Therapy — Fine Motor (Updated)",
    description: "Updated description.",
    durationMinutes: 50,
    cost: 1999.0,
  },
  null,
  2
);

const createDoctorBody = JSON.stringify(
  {
    firstName: "Ananya",
    lastName: "Menon",
    email: "dr.ananya@therapycenter.test",
    password: "Doctor@123",
    phoneNumber: "+919812345678",
    specialization: "Pediatric Occupational Therapy",
    bio: "10+ years experience with developmental delays.",
    availableDays: "Mon,Tue,Wed,Thu,Fri",
    startTime: "09:00:00",
    endTime: "17:00:00",
  },
  null,
  2
);

const updateDoctorBody = JSON.stringify(
  {
    firstName: "Ananya",
    lastName: "Menon",
    phoneNumber: "+919812345678",
    specialization: "Pediatric OT & Sensory Integration",
    bio: "Updated bio.",
    availableDays: "Mon,Tue,Wed,Thu",
    startTime: "10:00:00",
    endTime: "16:00:00",
  },
  null,
  2
);

// Numeric IDs unquoted so Postman substitutes numbers (not strings)
const createSlotBody = `{
  "doctorId": {{doctorId}},
  "date": "{{slotDate}}",
  "startTime": "10:00:00",
  "endTime": "10:45:00"
}`;

const updateSlotBody = JSON.stringify(
  {
    date: "{{slotDate}}",
    startTime: "11:00:00",
    endTime: "11:45:00",
  },
  null,
  2
);

const createAppointmentBody = `{
  "patientId": {{patientId}},
  "doctorId": {{doctorId}},
  "therapyId": {{therapyId}},
  "slotId": {{slotId}},
  "receptionistId": null,
  "notes": "Initial assessment — guardian requested morning slot."
}`;

const updateAppointmentBody = JSON.stringify({ notes: "Updated notes after check-in." }, null, 2);

const walkInBody = `{
  "doctorId": {{doctorId}},
  "therapyId": {{therapyId}},
  "date": "{{slotDate}}",
  "startTime": "14:00:00",
  "endTime": "14:45:00",
  "firstName": "WalkIn",
  "lastName": "Patient",
  "notes": "Walk-in registration at desk."
}`;

const bookOnlineBody = `{
  "firstName": "Arjun",
  "lastName": "Verma",
  "dateOfBirth": "2016-03-20",
  "gender": "M",
  "medicalHistory": "Mild ASD; prior OT at another clinic.",
  "doctorId": {{doctorId}},
  "therapyId": {{therapyId}},
  "appointmentDate": "{{slotDate}}",
  "startTime": "10:00:00",
  "endTime": "10:45:00"
}`;

const createOrderBody = `{
  "appointmentId": {{appointmentId}}
}`;

const verifyPaymentBody = `{
  "appointmentId": {{appointmentId}},
  "razorpayOrderId": "{{razorpayOrderId}}",
  "razorpayPaymentId": "{{razorpayPaymentId}}",
  "razorpaySignature": "{{razorpaySignature}}"
}`;

const updateUserBody = JSON.stringify(
  {
    firstName: "Ravi",
    lastName: "Kumar",
    email: "reception.ravi@example.com",
    phoneNumber: "+919876543211",
    password: null,
  },
  null,
  2
);

const createFindingBody = `{
  "appointmentId": {{appointmentId}},
  "observations": "Improved grip strength; minor fatigue noted.",
  "recommendations": "Continue home exercises 3x weekly.",
  "nextSessionDate": "2026-06-22"
}`;

const updateFindingBody = JSON.stringify(
  {
    observations: "Updated observations after follow-up.",
    recommendations: "Add sensory brushing before tasks.",
    nextSessionDate: "2026-06-29",
  },
  null,
  2
);

const collection = {
  info: {
    _postman_id: "a1b2c3d4-therapy-center-api",
    name: "TherapyCenter API",
    description:
      "Therapy Center ASP.NET Core Web API — generated from controllers/DTOs in-repo.\n\n**Suggested flow (example)**\n1. **Login Admin** (seed: `admin@therapy.com` / `123456` from `AdminSeeder`) — saves `adminToken`.\n2. **Create Therapy**, **Create Doctor** — copy returned IDs into environment (`therapyId`, `doctorId`) if building data manually; with `DemoDataSeeder` many IDs already exist.\n3. **Register Receptionist** + **Login Receptionist** — **Create Slot** for `doctorId` + `slotDate`.\n4. **Register Guardian** + **Login Guardian** — **Book Online** or use **Create Appointment** (needs existing `patientId` / `slotId`).\n5. **Payment** — `create-order` then `verify` with Razorpay client values.\n6. **Reports Summary** (Admin).\n\n**Redis:** only `GET /api/therapy/prices` uses distributed cache key `therapy_prices` (30 min TTL; invalidated on therapy **update**).\n\n**Auth:** JWT Bearer. Seeded admin cannot be created via Register; use Login Admin.\n\n**HTTPS profile:** default `https://localhost:7243` (see `Properties/launchSettings.json`).",
    schema: "https://schema.getpostman.com/json/collection/v2.1.0/collection.json",
  },
  variable: [{ key: "baseUrl", value: "https://localhost:7243", type: "string" }],
  item: [
    {
      name: "Auth",
      item: [
        req("Register — Guardian", {
          method: "POST",
          urlRaw: "{{baseUrl}}/api/auth/register",
          description:
            "Creates a `User` with role **Guardian** (allowed: Receptionist, Doctor, Guardian). Returns `AuthResponseDto` with JWT.",
          bodyRaw: registerGuardianBody,
          tests: loginTests,
        }),
        req("Register — Receptionist", {
          method: "POST",
          urlRaw: "{{baseUrl}}/api/auth/register",
          description: "Creates a **Receptionist** user (slot + walk-in flows).",
          bodyRaw: registerReceptionistBody,
          tests: loginTests,
        }),
        req("Login — Admin (seeded)", {
          method: "POST",
          urlRaw: "{{baseUrl}}/api/auth/login",
          description: "Uses `AdminSeeder` defaults: `admin@therapy.com` / `123456`. Saves `token` and `adminToken`.",
          bodyRaw: loginAdminBody,
          tests: loginTests,
        }),
        req("Login — Guardian", {
          method: "POST",
          urlRaw: "{{baseUrl}}/api/auth/login",
          description: "After Register Guardian (or if user exists). Saves `token` and `guardianToken`.",
          bodyRaw: loginGuardianBody,
          tests: loginTests,
        }),
        req("Login — Receptionist", {
          method: "POST",
          urlRaw: "{{baseUrl}}/api/auth/login",
          description: "Saves `token` and `receptionistToken`.",
          bodyRaw: loginReceptionistBody,
          tests: loginTests,
        }),
        req("Login — Doctor (after Create Doctor)", {
          method: "POST",
          urlRaw: "{{baseUrl}}/api/auth/login",
          description:
            "Use the **email/password** from **Create Doctor** (`dr.ananya@therapycenter.test` / `Doctor@123` in samples). Saves `doctorToken`.",
          bodyRaw: JSON.stringify({ email: "dr.ananya@therapycenter.test", password: "Doctor@123" }, null, 2),
          tests: loginTests,
        }),
      ],
    },
    {
      name: "Therapies",
      item: [
        req("Create Therapy", {
          method: "POST",
          urlRaw: "{{baseUrl}}/api/Therapy",
          description: "`[Authorize(Roles = Admin)]` — body: `CreateTherapyDto`. Test script saves `therapyId` from `TherapyResponseDto`.",
          bodyRaw: createTherapyBody,
          auth: bearer("{{adminToken}}"),
          tests: captureTherapyIdTests,
        }),
        req("Get All Therapies", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/Therapy",
          description: "Any authenticated user. Returns `TherapyResponseDto[]`.",
          auth: bearer("{{token}}"),
        }),
        req("Get Therapy Prices (Redis cache-aside)", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/therapy/prices",
          description:
            "Absolute route. Returns `TherapyPriceDto[]` (`Id`, `TherapyName`, `Price`). Service uses `IDistributedCache` key **therapy_prices** (30 min TTL).",
          auth: bearer("{{token}}"),
        }),
        req("Get Therapy By Id", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/Therapy/{{therapyId}}",
          description: "Replace `therapyId` environment variable.",
          auth: bearer("{{token}}"),
        }),
        req("Update Therapy", {
          method: "PUT",
          urlRaw: "{{baseUrl}}/api/Therapy/{{therapyId}}",
          description: "Admin only. Invalidates Redis key `therapy_prices`. Body: `UpdateTherapyDto`.",
          bodyRaw: updateTherapyBody,
          auth: bearer("{{adminToken}}"),
        }),
        req("Delete Therapy", {
          method: "DELETE",
          urlRaw: "{{baseUrl}}/api/Therapy/{{therapyId}}",
          description: "Admin only.",
          auth: bearer("{{adminToken}}"),
        }),
      ],
    },
    {
      name: "Doctors",
      item: [
        req("Create Doctor", {
          method: "POST",
          urlRaw: "{{baseUrl}}/api/Doctor",
          description:
            "Admin. Creates linked `User` (Doctor role) + `Doctor`. Body: `CreateDoctorDto` (`TimeOnly` as `HH:mm:ss`). Saves `doctorId`.",
          bodyRaw: createDoctorBody,
          auth: bearer("{{adminToken}}"),
          tests: captureDoctorIdTests,
        }),
        req("Get All Doctors", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/Doctor",
          description: "Authenticated. `DoctorResponseDto[]`.",
          auth: bearer("{{token}}"),
        }),
        req("Get Doctor By Id", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/Doctor/{{doctorId}}",
          auth: bearer("{{token}}"),
        }),
        req("Update Doctor", {
          method: "PUT",
          urlRaw: "{{baseUrl}}/api/Doctor/{{doctorId}}",
          bodyRaw: updateDoctorBody,
          auth: bearer("{{adminToken}}"),
        }),
        req("Delete Doctor", {
          method: "DELETE",
          urlRaw: "{{baseUrl}}/api/Doctor/{{doctorId}}",
          auth: bearer("{{adminToken}}"),
        }),
        req("Get Doctor List (Patient / Guardian)", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/Doctor/list",
          description:
            "`[Authorize(Roles = Patient,Guardian)]` — use `guardianToken` (or patient token if you add Patient users).",
          auth: bearer("{{guardianToken}}"),
        }),
      ],
    },
    {
      name: "Slots",
      item: [
        req("Create Slot", {
          method: "POST",
          urlRaw: "{{baseUrl}}/api/Slot",
          description: "Receptionist. Body: `CreateSlotDto` (`DateOnly` as `yyyy-MM-dd`). Saves `slotId`.",
          bodyRaw: createSlotBody,
          auth: bearer("{{receptionistToken}}"),
          tests: captureSlotIdTests,
        }),
        req("Get All Slots", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/Slot",
          description: "Admin or Receptionist.",
          auth: bearer("{{receptionistToken}}"),
        }),
        req("Get Slot By Id", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/Slot/{{slotId}}",
          auth: bearer("{{receptionistToken}}"),
        }),
        req("Get Slots By Doctor And Date", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/Slot/doctor/{{doctorId}}?date={{slotDate}}",
          description: "Required query: `date` (`DateOnly`).",
          auth: bearer("{{receptionistToken}}"),
        }),
        req("Update Slot", {
          method: "PUT",
          urlRaw: "{{baseUrl}}/api/Slot/{{slotId}}",
          bodyRaw: updateSlotBody,
          auth: bearer("{{receptionistToken}}"),
        }),
        req("Delete Slot", {
          method: "DELETE",
          urlRaw: "{{baseUrl}}/api/Slot/{{slotId}}",
          auth: bearer("{{receptionistToken}}"),
        }),
        req("Get Generated Slots By Doctor", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/Slot/doctor/{{doctorId}}/generated?date={{slotDate}}",
          description:
            "Optional query `therapyId` (not in raw URL): append `&therapyId={{therapyId}}` when needed.",
          auth: bearer("{{token}}"),
        }),
      ],
    },
    {
      name: "Appointments",
      item: [
        req("Create Appointment (slot-based)", {
          method: "POST",
          urlRaw: "{{baseUrl}}/api/Appointment",
          description: "Guardian or Receptionist. Body: `CreateAppointmentDto`. Saves `appointmentId`.",
          bodyRaw: createAppointmentBody,
          auth: bearer("{{guardianToken}}"),
          tests: captureAppointmentIdTests,
        }),
        req("Get All Appointments", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/Appointment",
          description: "Admin only.",
          auth: bearer("{{adminToken}}"),
        }),
        req("Get Appointment By Id", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/Appointment/{{appointmentId}}",
          auth: bearer("{{token}}"),
        }),
        req("Update Appointment", {
          method: "PUT",
          urlRaw: "{{baseUrl}}/api/Appointment/{{appointmentId}}",
          description: "Receptionist or Admin. Body: `UpdateAppointmentDto`.",
          bodyRaw: updateAppointmentBody,
          auth: bearer("{{receptionistToken}}"),
        }),
        req("Delete Appointment", {
          method: "DELETE",
          urlRaw: "{{baseUrl}}/api/Appointment/{{appointmentId}}",
          description: "Admin only.",
          auth: bearer("{{adminToken}}"),
        }),
        req("Complete Appointment", {
          method: "PUT",
          urlRaw: "{{baseUrl}}/api/Appointment/{{appointmentId}}/complete",
          description: "Doctor only.",
          auth: bearer("{{doctorToken}}"),
        }),
        req("Cancel Appointment", {
          method: "PUT",
          urlRaw: "{{baseUrl}}/api/Appointment/{{appointmentId}}/cancel",
          description: "Receptionist, Admin, or Doctor.",
          auth: bearer("{{receptionistToken}}"),
        }),
        req("Create Walk-In Appointment", {
          method: "POST",
          urlRaw: "{{baseUrl}}/api/Appointment/walkin",
          description: "Receptionist. Body: `CreateWalkInAppointmentDto`. Saves `appointmentId`.",
          bodyRaw: walkInBody,
          auth: bearer("{{receptionistToken}}"),
          tests: captureAppointmentIdTests,
        }),
        req("Get Appointments For Doctor (JWT)", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/Appointment/doctor",
          description: "Doctor. Resolves doctor from `NameIdentifier` claim.",
          auth: bearer("{{doctorToken}}"),
        }),
        req("Book Online Appointment", {
          method: "POST",
          urlRaw: "{{baseUrl}}/api/Appointment/online",
          description:
            "Guardian. Body: `BookOnlineAppointmentDto` (creates/links patient flow in service). Saves `appointmentId`.",
          bodyRaw: bookOnlineBody,
          auth: bearer("{{guardianToken}}"),
          tests: captureAppointmentIdTests,
        }),
        req("Get My Appointments (Guardian)", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/Appointment/my",
          description: "Guardian.",
          auth: bearer("{{guardianToken}}"),
        }),
      ],
    },
    {
      name: "Patients",
      item: [
        req("Get My Patients (Guardian)", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/Patients/mine",
          description: "`PatientSummaryDto[]` for current guardian. Test script saves first `patientId`.",
          auth: bearer("{{guardianToken}}"),
          tests: captureFirstPatientIdTests,
        }),
      ],
    },
    {
      name: "Guardian Dashboard",
      description:
        "Shortcut folder for **Guardian** JWT (`guardianToken`). Same URLs as elsewhere; no extra Redis-only APIs beyond therapy prices.",
      item: [
        req("[Guardian] My Patients", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/Patients/mine",
          auth: bearer("{{guardianToken}}"),
          tests: captureFirstPatientIdTests,
        }),
        req("[Guardian] Book Online", {
          method: "POST",
          urlRaw: "{{baseUrl}}/api/Appointment/online",
          bodyRaw: bookOnlineBody,
          auth: bearer("{{guardianToken}}"),
          tests: captureAppointmentIdTests,
        }),
        req("[Guardian] My Appointments", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/Appointment/my",
          auth: bearer("{{guardianToken}}"),
        }),
        req("[Guardian] Therapy Prices (cached)", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/therapy/prices",
          auth: bearer("{{guardianToken}}"),
        }),
        req("[Guardian] Doctor List For Booking", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/Doctor/list",
          auth: bearer("{{guardianToken}}"),
        }),
        req("[Guardian] Payment Create Order", {
          method: "POST",
          urlRaw: "{{baseUrl}}/api/Payment/create-order",
          bodyRaw: createOrderBody,
          auth: bearer("{{guardianToken}}"),
          tests: captureRazorpayOrderIdTests,
        }),
        req("[Guardian] Payment Verify", {
          method: "POST",
          urlRaw: "{{baseUrl}}/api/Payment/verify",
          bodyRaw: verifyPaymentBody,
          auth: bearer("{{guardianToken}}"),
        }),
      ],
    },
    {
      name: "Reports",
      item: [
        req("Get Reports Summary", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/Reports/summary",
          description: "Admin. Returns summary DTO from `IReportService.GetSummaryAsync`.",
          auth: bearer("{{adminToken}}"),
        }),
      ],
    },
    {
      name: "Payments",
      item: [
        req("Create Razorpay Order", {
          method: "POST",
          urlRaw: "{{baseUrl}}/api/Payment/create-order",
          description:
            "Admin or Guardian. Body: `CreateOrderDto` (`appointmentId`). Saves `razorpayOrderId` from response `orderId` for verify step.",
          bodyRaw: createOrderBody,
          auth: bearer("{{guardianToken}}"),
          tests: captureRazorpayOrderIdTests,
        }),
        req("Verify Razorpay Payment", {
          method: "POST",
          urlRaw: "{{baseUrl}}/api/Payment/verify",
          description: "Body: `VerifyPaymentDto` — fill from Razorpay checkout response.",
          bodyRaw: verifyPaymentBody,
          auth: bearer("{{guardianToken}}"),
        }),
      ],
    },
    {
      name: "Admin",
      item: [
        {
          name: "Users",
          item: [
            req("List All Users", {
              method: "GET",
              urlRaw: "{{baseUrl}}/api/Users",
              auth: bearer("{{adminToken}}"),
            }),
            req("List Receptionists", {
              method: "GET",
              urlRaw: "{{baseUrl}}/api/Users/receptionists",
              auth: bearer("{{adminToken}}"),
            }),
            req("Update User", {
              method: "PUT",
              urlRaw: "{{baseUrl}}/api/Users/{{userId}}",
              description: "Body: `UpdateUserDto`. Set `userId` from login or list users.",
              bodyRaw: updateUserBody,
              auth: bearer("{{adminToken}}"),
            }),
            req("Delete User", {
              method: "DELETE",
              urlRaw: "{{baseUrl}}/api/Users/{{userId}}",
              description: "Dangerous — use a disposable test user id.",
              auth: bearer("{{adminToken}}"),
            }),
          ],
        },
        req("Reports Summary", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/Reports/summary",
          auth: bearer("{{adminToken}}"),
        }),
        req("Test — Admin Only", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/Test/admin-only",
          auth: bearer("{{adminToken}}"),
        }),
      ],
    },
    {
      name: "Doctor Findings",
      item: [
        req("Create Doctor Finding", {
          method: "POST",
          urlRaw: "{{baseUrl}}/api/DoctorFinding",
          description: "Doctor. Body: `CreateDoctorFindingDto`. Saves `doctorFindingId` from `findingId`.",
          bodyRaw: createFindingBody,
          auth: bearer("{{doctorToken}}"),
          tests: captureDoctorFindingIdTests,
        }),
        req("Get All Doctor Findings", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/DoctorFinding",
          description: "Admin or Receptionist.",
          auth: bearer("{{adminToken}}"),
        }),
        req("Get Doctor Finding By Id", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/DoctorFinding/{{doctorFindingId}}",
          auth: bearer("{{token}}"),
        }),
        req("Get Doctor Finding By Appointment", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/DoctorFinding/appointment/{{appointmentId}}",
          auth: bearer("{{token}}"),
        }),
        req("Update Doctor Finding", {
          method: "PUT",
          urlRaw: "{{baseUrl}}/api/DoctorFinding/{{doctorFindingId}}",
          bodyRaw: updateFindingBody,
          auth: bearer("{{doctorToken}}"),
        }),
        req("Delete Doctor Finding", {
          method: "DELETE",
          urlRaw: "{{baseUrl}}/api/DoctorFinding/{{doctorFindingId}}",
          description: "Admin only.",
          auth: bearer("{{adminToken}}"),
        }),
      ],
    },
    {
      name: "Diagnostics",
      item: [
        req("Test — Public", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/Test/public",
          description: "No JWT.",
        }),
        req("Test — Secure", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/Test/secure",
          description: "Any authenticated user.",
          auth: bearer("{{token}}"),
        }),
        req("Test — Who Am I", {
          method: "GET",
          urlRaw: "{{baseUrl}}/api/Test/whoami",
          auth: bearer("{{token}}"),
        }),
      ],
    },
  ],
};

const outPath = path.join(outDir, "TherapyCenter.postman_collection.json");
fs.writeFileSync(outPath, JSON.stringify(collection, null, 2), "utf8");
console.log("Wrote", outPath);
