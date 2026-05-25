const roomService = require('./room.service');
const departmentService = require('../department/department.service');
const { renderWithLayout } = require('../../utils/render');

const listRooms = async (req, res) => {
  const rooms = await roomService.getAllRooms();
  renderWithLayout(res, 'pages/admin/rooms/list', {
    title: 'Quản lý phòng',
    rooms
  });
};

const createRoomForm = async (req, res) => {
  const departments = await departmentService.getAllDepartments();
  renderWithLayout(res, 'pages/admin/rooms/create', {
    title: 'Thêm phòng',
    departments
  });
};

const createRoom = async (req, res) => {
  await roomService.createRoom(req.body);
  res.redirect('/admin/rooms');
};

const editRoomForm = async (req, res) => {
  const room = await roomService.getRoomById(req.params.id);
  const departments = await departmentService.getAllDepartments();
  renderWithLayout(res, 'pages/admin/rooms/edit', {
    title: 'Cập nhật phòng',
    room,
    departments
  });
};

const updateRoom = async (req, res) => {
  await roomService.updateRoom(req.params.id, req.body);
  res.redirect('/admin/rooms');
};

const deleteRoom = async (req, res) => {
  await roomService.deleteRoom(req.params.id);
  res.redirect('/admin/rooms');
};

module.exports = {
  listRooms,
  createRoomForm,
  createRoom,
  editRoomForm,
  updateRoom,
  deleteRoom,
};