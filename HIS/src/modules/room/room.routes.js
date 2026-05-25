const express = require('express');
const router = express.Router();
const roomController = require('./room.controller');
const { protect } = require('../../middlewares/auth.middleware');
const requireRole = require('../../middlewares/requireRole.js');
const { PERMISSIONS } = require('../../config/permissions');

router.use(protect, requireRole(PERMISSIONS.MANAGE_ROOMS));

router.get('/', roomController.listRooms);
router.get('/new', roomController.createRoomForm);
router.post('/', roomController.createRoom);
router.get('/:id/edit', roomController.editRoomForm);
router.post('/:id', roomController.updateRoom);
router.post('/:id/delete', roomController.deleteRoom);

module.exports = router;