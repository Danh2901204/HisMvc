const receptionRoutes = require('./src/modules/reception/reception.routes');
const doctorRoutes = require('./src/modules/doctor/doctor.routes');
const labRoutes = require('./src/modules/lab/lab.routes');
const adminRoutes = require('./src/modules/admin/admin.routes');

app.use('/reception', receptionRoutes);
app.use('/doctors', doctorRoutes);
app.use('/lab', labRoutes);
app.use('/admin', adminRoutes);