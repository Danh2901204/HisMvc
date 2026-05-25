const express = require('express');
const router = express.Router();
const authController = require('./auth.controller');

// @route   GET /auth/login
// @desc    Render login page
// @access  Public
router.get('/login', authController.renderLoginPage);

// @route   POST /auth/login
// @desc    Login user
// @access  Public
router.post('/login', authController.loginUser);

// @route   GET /auth/logout
// @desc    Logout user
// @access  Public
router.get('/logout', authController.logoutUser);

module.exports = router;