// ... existing code ...
const receptionRoutes = require('./modules/reception/reception.routes');
const departmentRoutes = require('./modules/department/department.routes');
const roomRoutes = require('./modules/room/room.routes');
const bedRoutes = require('./modules/bed/bed.routes');
const drugRoutes = require('./modules/drug/drug.routes');
const doctorRoutes = require('./modules/doctor/doctor.routes');
const { protect } = require('./middlewares/auth.middleware');
const { setUser } = require('./middlewares/user.middleware');

const app = express();
// ... existing code ...
app.use('/admin/beds', bedRoutes);
app.use('/admin/drugs', drugRoutes);
app.use('/reception', receptionRoutes);
app.use('/doctor', doctorRoutes);


// Error handling middleware
app.use((err, req, res, next) => {
// ... existing code ...
    res.status(500).send(error.message);
});