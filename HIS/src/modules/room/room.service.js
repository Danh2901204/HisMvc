const Room = require('./models/room.model');

const getAllRooms = async () => {
  return await Room.find().populate('department');
};

const getRoomById = async (id) => {
  return await Room.findById(id).populate('department');
};

const createRoom = async (roomData) => {
  const room = new Room(roomData);
  return await room.save();
};

const updateRoom = async (id, roomData) => {
  return await Room.findByIdAndUpdate(id, roomData, { new: true });
};

const deleteRoom = async (id) => {
  return await Room.findByIdAndDelete(id);
};

module.exports = {
  getAllRooms,
  getRoomById,
  createRoom,
  updateRoom,
  deleteRoom,
};