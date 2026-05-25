const bedService = require('./bed.service');
const roomService = require('../room/room.service');
const { renderWithLayout } = require('../../utils/render');

const listBeds = async (req, res) => {
  const beds = await bedService.getAllBeds();
  renderWithLayout(res, 'pages/admin/beds/list', {
    title: 'Quản lý giường',
    beds
  });
};

const createBedForm = async (req, res) => {
  const rooms = await roomService.getAllRooms();
  renderWithLayout(res, 'pages/admin/beds/create', {
    title: 'Thêm giường',
    rooms
  });
};

const createBed = async (req, res) => {
  await bedService.createBed(req.body);
  res.redirect('/admin/beds');
};

const editBedForm = async (req, res) => {
  const bed = await bedService.getBedById(req.params.id);
  const rooms = await roomService.getAllRooms();
  renderWithLayout(res, 'pages/admin/beds/edit', {
    title: 'Cập nhật giường',
    bed,
    rooms
  });
};

const updateBed = async (req, res) => {
  await bedService.updateBed(req.params.id, req.body);
  res.redirect('/admin/beds');
};

const deleteBed = async (req, res) => {
  await bedService.deleteBed(req.params.id);
  res.redirect('/admin/beds');
};

module.exports = {
  listBeds,
  createBedForm,
  createBed,
  editBedForm,
  updateBed,
  deleteBed,
};