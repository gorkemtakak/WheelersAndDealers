﻿using AutoMapper;
using Common;
using Data;
using Domain;
using Menager.Dtos.RequestDto;
using Menager.Dtos.ResponseDto;
using System.Runtime.InteropServices;

namespace Menager
{
    public class SparePartsMenager : ISpareParts
    {
        private readonly IMapper _mapper;
        private readonly ISparePartsData _sparePartsData;
        public SparePartsMenager(IMapper mapper, ISparePartsData sparePartsData)
        {
            _mapper = mapper;
            _sparePartsData = sparePartsData;
        }
        public async Task CreateItem(CreateItemRequestDto requestDto)
        {
            var admin = await _sparePartsData.getAdminById(requestDto.UserId);
            if (admin == null)
            {
                throw new Exception("You are not allowed to do this action");
            }

            Item item = new Item(requestDto.ItemName, requestDto.ItemDescription, requestDto.ItemTypeId, requestDto.CarBrandId, requestDto.ProductCode, requestDto.GuaranteeTime);
            await _sparePartsData.CreateItem(item);
            await _sparePartsData.PersistAsync();
        }

        public async Task CreateNotification(CreateNotificationRequestDto requestDto)
        {
            var notification = new Notification(requestDto.DestinatonPersonId, requestDto.UserId, requestDto.Description);
            await _sparePartsData.CreateNotification(notification);
            await _sparePartsData.PersistAsync();
        }

        public async Task CreatePurchaseOrder(CreatePurchaseOrderRequestDto requestDto)
        {
            var user = await _sparePartsData.getUserById(requestDto.UserId);
            if (user == null)
            {
                throw new Exception("You are not allowed to do this action");
            }

            PurchaseOrder purchaseOrder = new PurchaseOrder(requestDto.UserId, requestDto.PurchaseOrderPrice, requestDto.TitleOfDestinationAddress, requestDto.TitleOfBill, requestDto.DestinationAddressDescription, requestDto.BillDescription);
            user.AddPurchaseOrder(purchaseOrder);
            requestDto.PurchaseOrderDetailList.ForEach(x => purchaseOrder.AddPurchaseOrderDetail(x.SupplierItemId, x.ItemId, x.Quantity, x.PurchaseOrderDetailPrice));
            await _sparePartsData.CreatePurchaseOrder(purchaseOrder);
        }

        public async Task CreateSupplier(CreateSupplierRequestDto requestDto)
        {
            var admin = await _sparePartsData.getAdminById(requestDto.UserId);
            if (admin == null)
            {
                throw new Exception("You are not allowed to do this action");
            }

            var supplier = new Supplier(requestDto.SupplierName, requestDto.SupplierDescription, requestDto.Email, requestDto.Password, requestDto.Phone, requestDto.SupplierLocation);
            await _sparePartsData.CreateSupplier(supplier);
            await _sparePartsData.PersistAsync();
        }

        public async Task CreateUser(CreateUserRequestDto requestDto)
        {
            var user = new User(requestDto.Name, requestDto.Surname, requestDto.Password);
            await _sparePartsData.CreateUser(user);
            await _sparePartsData.PersistAsync();
        }

        public async Task<GetItemByIdResponseDto> GetItemById(Guid id)
        {
            var item = await _sparePartsData.getItemById(id);
            return _mapper.Map<GetItemByIdResponseDto>(item);
        }

        public async Task<List<GetItemByParametersResponseDto>> GetItemByParameters(string? searchText, bool? isActive, int skip, int take)
        {
            var itemList = await _sparePartsData.getItemByParameters(searchText, isActive, skip, take);
            return _mapper.Map<List<GetItemByParametersResponseDto>>(itemList);
        }

        public async Task<GetNotificationResponseDto> GetNotificationsById(Guid id)
        {
            var notification = await _sparePartsData.getNotificationById(id);
            return _mapper.Map<GetNotificationResponseDto>(notification);
        }

        public async Task<GetPurchaseOrderByIdResponseDto> GetPurchaseOrderById(Guid id)
        {
            var purchaseOrder = await _sparePartsData.getPurchaseOrderById(id);
            return _mapper.Map<GetPurchaseOrderByIdResponseDto>(purchaseOrder);
        }

        public async Task<GetUserByParametersResponseDto> GetUsers()
        {
            var users = await _sparePartsData.getUsers();
            return _mapper.Map<GetUserByParametersResponseDto>(users);
        }

        public async Task<Guid?> LoginForAdmin(string email, string password)
        {
            return await _sparePartsData.GetAdminByMailAndPassword(email, password);
        }
        public async Task<Guid?> LoginForUser(string email, string password)
        {
            return await _sparePartsData.GetUserByMailAndPassword(email, password);
        }
        public async Task<Guid?> LoginForSupplier(string email, string password)
        {
            return await _sparePartsData.GetUserByMailAndPassword(email, password);
        }

        public async Task UpdateItem(UpdateItemRequestDto requestDto)
        {
            var admin = await _sparePartsData.getAdminById(requestDto.UserId);
            if (admin == null)
            {
                throw new Exception("You are not allowed to do this action");
            }
            Item item = await _sparePartsData.getItemById(requestDto.ItemId);
            item.UpdateItem(requestDto.ItemName, requestDto.ItemDescription, requestDto.ItemType, requestDto.ProductCode, requestDto.GuaranteeTime, requestDto.IsActive);
            await _sparePartsData.PersistAsync();
        }

        public async Task CreateItemSupplierRelation(CreateSupplierItemRelationRequestDto requestDto)
        {
            var admin = await _sparePartsData.getAdminById(requestDto.UserId);
            if (admin == null)
            {
                throw new Exception("You are not allowed to do this action");
            }
            Item item = await _sparePartsData.getItemById(requestDto.ItemId);
            item.AddSupplierItem(requestDto.SupplierId, requestDto.Price, requestDto.SupplierName);
            await _sparePartsData.PersistAsync();


        }

        public async Task UpdateNotification(UpdateNotificationRequestDto requestDto)
        {
            Notification notification = await _sparePartsData.getNotificationById(requestDto.Id);
            notification.UpdateNotification(requestDto.Description, requestDto.HasRead);
            await _sparePartsData.PersistAsync();
        }

        public async Task UpdatePurchaseOrder(UpdatePurchaseOrderRequestDto requestDto)
        {
            var purchaseOrder = await _sparePartsData.getPurchaseOrderById(requestDto.Id);
            if (purchaseOrder is null)
            {
                throw new Exception("Purchase order could not be found");
            }
            foreach (var purchaseOrderDetail in purchaseOrder.PurchaseOrderDetail)
            {
                var detail = requestDto.UpdatePurchaseOrderList.FirstOrDefault(x => x.Id == purchaseOrderDetail.Id);
                if (detail == null)
                {
                    throw new Exception("Related detail could not be found");
                }
                purchaseOrderDetail.UpdatePurchaseOrderDetail(detail.CargoStatusId, detail.DestinationBranch ?? " ", detail.CargoNumber ?? " ", detail.PurchaseOrderStatusId);
            }
            await _sparePartsData.PersistAsync();
        }

        public async Task UpdateSupplier(UpdateSuppplierRequestDto requestDto)
        {
            var admin = await _sparePartsData.getAdminById(requestDto.UserId);
            if (admin == null)
            {
                throw new Exception("You are not allowed to do this action");
            }
            var supplier = await _sparePartsData.getSupplierById(requestDto.Id);
            if (supplier == null)
            {
                throw new Exception("Supplier could not be found");
            }

            supplier.UpdateSupplier(requestDto.SupplierName, requestDto.SupplierDescription, requestDto.Phone, requestDto.SupplierLocation);
            await _sparePartsData.PersistAsync();
        }

        public async Task UpdateSupplierItemRequestDto(UpdateSupplierItemRequestDto requestDto)
        {
            var admin = await _sparePartsData.getAdminById(requestDto.UserId);
            if (admin == null)
            {
                throw new Exception("You are not allowed to do this action");
            }
            var supplierItem = await _sparePartsData.getSupplierItemById(requestDto.Id);
            if (supplierItem == null)
            {
                throw new Exception("Supplier could not be found");
            }

            supplierItem.UpdateSupplierItem(requestDto.IsActive, requestDto.Price, requestDto.SupplierName, requestDto.ItemName);
            await _sparePartsData.PersistAsync();
        }

        public async Task UpdateUser(UpdateUserRequestDto requestDto)
        {
            var user = await _sparePartsData.getUserById(requestDto.Id);
            if (user == null)
            {
                throw new Exception("User could not be found");
            }
            user.UpdateUser(requestDto.Name, requestDto.Surname, requestDto.Password);
            await _sparePartsData.PersistAsync();
        }

        public async Task UploadImage(UploadImageRequestDto requestDto)
        {
            var item = await _sparePartsData.getItemById(requestDto.Id);
            if (item is null)
            {
                throw new Exception("Item could not be found");
            }
            item.UpdateImage(item.ItemName, requestDto.ImageData);
            await _sparePartsData.PersistAsync();
        }
        public async Task DeleteImage(Guid id)
        {
            var item = await _sparePartsData.getItemById(id);
            if (item is null)
            {
                throw new Exception("Item could not be found");
            }
            item.UpdateImage(null, null);
            await _sparePartsData.PersistAsync();
        }

    }
}
