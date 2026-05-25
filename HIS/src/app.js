const express = require('express');
const path = require('path');
const cookieParser = require('cookie-parser');
const env = require('./config/env');
const authRoutes = require('./modules/auth/auth.routes');
const adminRoutes = require('./modules/admin/admin.routes');
const receptionRoutes = require('./modules/reception/reception.routes');
const departmentRoutes = require('./modules/department/department.routes');
const roomRoutes = require('./modules/room/room.routes');
const bedRoutes = require('./modules/bed/bed.routes');
const drugRoutes = require('./modules/drug/drug.routes');
const { protect } = require('./middlewares/auth.middleware');
const { setUser } = require('./middlewares/user.middleware');

const app = express();

// Middlewares
app.use(express.json());
app.use(express.urlencoded({ extended: false }));
app.use(cookieParser());
app.use(setUser);
app.use(express.static(path.join(__dirname, 'public')));

// View Engine Setup
app.set('views', path.join(__dirname, 'views'));
app.set('view engine', 'ejs');

// Routes
app.get('/', (req, res) => {
  res.render('layouts/main', {
    body: 'pages/home', // This will render views/pages/home.ejs
    title: 'Home Page'
  });
});
app.get('/dashboard', protect, (req, res) => {
    res.render('layouts/main', {
      body: 'pages/dashboard',
      title: 'Dashboard'
    });
});
app.use('/auth', authRoutes);
app.use('/admin', adminRoutes);
app.use('/admin/departments', departmentRoutes);
app.use('/admin/rooms', roomRoutes);
app.use('/admin/beds', bedRoutes);
app.use('/admin/drugs', drugRoutes);
app.use('/reception', receptionRoutes);


// Error handling middleware
app.use((err, req, res, next) => {
  console.error(err.stack);
  res.status(500).send('Something broke!');
});

module.exports = app;