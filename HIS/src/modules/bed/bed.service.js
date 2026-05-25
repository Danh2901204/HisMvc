const Bed = require('./models/bed.model');

const getAllBeds = async () => {
  return await Bed.find().populate('room');
};

const getBedById = async (id) => {
  return await Bed.findById(id).populate('room');
};

const createBed = async (bedData) => {
  const bed = new Bed(bedData);
  return await bed.save();
};

const updateBed = async (id, bedData) => {
  return await Bed.findByIdAndUpdate(id, bedData, { new: true });
};

const deleteBed = async (id) => {
  return await Bed.findByIdAndDelete(id);
};

module.exports = {
  getAllBeds,
  getBedById,
  createBed,
  updateBed,
  deleteBed,
};