const receptionService = require('./reception.service');

function renderWithLayout(req, res, view, data = {}) {
  res.render('layouts/main', {
    body: `pages/reception/${view}`,
    ...data
  });
}

exports.listPatients = async (req, res) => {
    const searchTerm = req.query.search || '';
    const patients = await receptionService.getPatients(searchTerm);
    renderWithLayout(req, res, "patient-list", {
        title: "Danh sách bệnh nhân",
        patients,
        searchTerm,
    });
};

exports.newPatientForm = (req, res) => {
  renderWithLayout(req, res, "patient-create", {
    title: "Tạo hồ sơ bệnh nhân"
  });
};

exports.createPatient = async (req, res) => {
  const { name, dob, phone, cccd } = req.body;

  await receptionService.createPatient({
    name: (name || "").trim(),
    dob,
    phone: (phone || "").trim(),
    cccd: (cccd || "").trim()
  });

  res.redirect("/reception/patients");
};

exports.newVisitForm = async (req, res) => {
  const patients = await receptionService.getPatients();
  renderWithLayout(req, res, "visit-register", {
    title: "Đăng ký lượt khám",
    patients
  });
};

exports.createVisit = async (req, res) => {
  const { patientId, dept, room, reason, priority } = req.body;

  await receptionService.createVisit({
    patient: patientId,
    department: dept,
    room,
    reason,
    priority,
    status: "Chờ khám",
  });

  res.redirect("/reception/patients");
};